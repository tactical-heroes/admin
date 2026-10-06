using System.Security.Cryptography;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Npgsql;

using PANiXiDA.Core.Ef.Migrator;

using TacticalHeroes.Admin.Infrastructure.DataProtection;

using Testcontainers.PostgreSql;

namespace TacticalHeroes.Admin.IntegrationTests.Infrastructure.DataProtection;

public sealed class AdminDataProtectionPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("tactical_heroes_dev")
        .WithUsername("admin")
        .WithPassword("synthetic-test-password")
        .Build();

    public ValueTask InitializeAsync() => new(_database.StartAsync(TestContext.Current.CancellationToken));

    public ValueTask DisposeAsync() => _database.DisposeAsync();

    [Fact(DisplayName = "RunMigrationsAsync should preserve the schema when migration is applied twice")]
    public async Task RunMigrationsAsync_Should_PreserveSchema_When_MigrationIsAppliedTwice()
    {
        await using var services = CreateServices(_database.GetConnectionString());
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AdminDataProtectionDbContext>();

        await MigrateAsync(_database.GetConnectionString());
        await MigrateAsync(_database.GetConnectionString());

        (await database.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken)).ShouldHaveSingleItem();
        database.Database.HasPendingModelChanges().ShouldBeFalse();
        (await database.DataProtectionKeys.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact(DisplayName = "Unprotect should recover protected data when application service provider is recreated")]
    public async Task Unprotect_Should_RecoverProtectedData_When_ApplicationServiceProviderIsRecreated()
    {
        string protectedValue;
        int keyCount;
        await using (var original = CreateServices(_database.GetConnectionString()))
        {
            await using var scope = original.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<AdminDataProtectionDbContext>();
            await MigrateAsync(_database.GetConnectionString());
            var protector = original.GetRequiredService<IDataProtectionProvider>().CreateProtector("antiforgery-test");
            protectedValue = protector.Protect("synthetic-cookie");
            keyCount = await database.DataProtectionKeys.CountAsync(TestContext.Current.CancellationToken);
            keyCount.ShouldBeGreaterThan(0);
        }

        await using var replacement = CreateServices(_database.GetConnectionString());
        var replacementProtector = replacement.GetRequiredService<IDataProtectionProvider>().CreateProtector("antiforgery-test");

        string restoredValue = replacementProtector.Unprotect(protectedValue);

        restoredValue.ShouldBe("synthetic-cookie");
        await using var replacementScope = replacement.CreateAsyncScope();
        var replacementDatabase = replacementScope.ServiceProvider.GetRequiredService<AdminDataProtectionDbContext>();
        (await replacementDatabase.DataProtectionKeys.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(keyCount);
    }

    [Fact(DisplayName = "Unprotect should reject another environment's data when databases are isolated")]
    public async Task Unprotect_Should_RejectAnotherEnvironmentsData_When_DatabasesAreIsolated()
    {
        await using var connection = new NpgsqlConnection(_database.GetConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new NpgsqlCommand("CREATE DATABASE tactical_heroes_prod", connection);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        string productionConnection = new NpgsqlConnectionStringBuilder(_database.GetConnectionString())
        {
            Database = "tactical_heroes_prod"
        }.ConnectionString;
        await using var development = CreateServices(_database.GetConnectionString());
        await using var production = CreateServices(productionConnection);
        await MigrateAsync(_database.GetConnectionString());
        await MigrateAsync(productionConnection);
        var developmentProtector = development.GetRequiredService<IDataProtectionProvider>().CreateProtector("same-purpose");
        var productionProtector = production.GetRequiredService<IDataProtectionProvider>().CreateProtector("same-purpose");
        string developmentValue = developmentProtector.Protect("development-cookie");

        Should.Throw<CryptographicException>(() => productionProtector.Unprotect(developmentValue));

        string productionValue = productionProtector.Protect("production-cookie");
        productionProtector.Unprotect(productionValue).ShouldBe("production-cookie");
    }

    [Fact(DisplayName = "ValidateRequestAsync should accept existing antiforgery tokens when application restarts")]
    public async Task ValidateRequestAsync_Should_AcceptExistingAntiforgeryTokens_When_ApplicationRestarts()
    {
        AntiforgeryTokenSet tokens;
        string cookieName;
        string? applicationDiscriminator;
        await using (var original = CreateApplication(_database.GetConnectionString()))
        {
            await MigrateAsync(_database.GetConnectionString());
            var context = new DefaultHttpContext { RequestServices = original.Services };
            tokens = original.Services.GetRequiredService<IAntiforgery>().GetTokens(context);
            cookieName = original.Services.GetRequiredService<IOptions<AntiforgeryOptions>>().Value.Cookie.Name!;
            applicationDiscriminator = original.Services.GetRequiredService<IOptions<DataProtectionOptions>>().Value.ApplicationDiscriminator;
            applicationDiscriminator.ShouldNotBeNullOrWhiteSpace();
        }

        await using var replacement = CreateApplication(_database.GetConnectionString());
        var request = new DefaultHttpContext { RequestServices = replacement.Services };
        request.Request.Method = HttpMethods.Post;
        request.Request.Headers.Cookie = $"{cookieName}={tokens.CookieToken}";
        request.Request.Headers["RequestVerificationToken"] = tokens.RequestToken;

        var exception = await Record.ExceptionAsync(() => replacement.Services.GetRequiredService<IAntiforgery>().ValidateRequestAsync(request));

        exception.ShouldBeNull();
        replacement.Services.GetRequiredService<IOptions<DataProtectionOptions>>().Value.ApplicationDiscriminator
            .ShouldBe(applicationDiscriminator);
    }

    [Fact(DisplayName = "RunMigrationsAsync should preserve API data and history when database is shared with api")]
    public async Task RunMigrationsAsync_Should_PreserveApiDataAndHistory_When_DatabaseIsSharedWithApi()
    {
        await using var connection = new NpgsqlConnection(_database.GetConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var seed = new NpgsqlCommand("""
            CREATE SCHEMA identity;
            CREATE TABLE identity.__ef_migrations_history (
                "MigrationId" text PRIMARY KEY,
                "ProductVersion" text NOT NULL
            );
            INSERT INTO identity.__ef_migrations_history VALUES ('existing-api-migration', '10.0.12');
            CREATE TABLE identity.data_protection_keys (id integer PRIMARY KEY, xml text);
            INSERT INTO identity.data_protection_keys VALUES (1, 'synthetic-api-key');
            """, connection);
        await seed.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        await using var services = CreateServices(_database.GetConnectionString());

        await MigrateAsync(_database.GetConnectionString());
        await MigrateAsync(_database.GetConnectionString());
        var protector = services.GetRequiredService<IDataProtectionProvider>().CreateProtector("admin-cookie");
        string protectedValue = protector.Protect("synthetic-admin-cookie");

        protector.Unprotect(protectedValue).ShouldBe("synthetic-admin-cookie");
        await using var apiKey = new NpgsqlCommand("SELECT xml FROM identity.data_protection_keys WHERE id = 1", connection);
        (await apiKey.ExecuteScalarAsync(TestContext.Current.CancellationToken)).ShouldBe("synthetic-api-key");
        await using var apiHistory = new NpgsqlCommand("SELECT \"MigrationId\" FROM identity.__ef_migrations_history", connection);
        (await apiHistory.ExecuteScalarAsync(TestContext.Current.CancellationToken)).ShouldBe("existing-api-migration");
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AdminDataProtectionDbContext>();
        (await database.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken)).ShouldHaveSingleItem();
        (await database.DataProtectionKeys.CountAsync(TestContext.Current.CancellationToken)).ShouldBeGreaterThan(0);
    }

    private static WebApplication CreateApplication(string connectionString)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = AppContext.BaseDirectory,
            ApplicationName = typeof(AdminDataProtectionDbContext).Assembly.GetName().Name,
            EnvironmentName = Environments.Production
        });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:PostgreSqlConnectionString"] = connectionString
        });
        builder.Services.AddAntiforgery().AddAdminDataProtection(builder.Configuration);
        return builder.Build();
    }

    private static ServiceProvider CreateServices(string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:PostgreSqlConnectionString"] = connectionString
        }).Build();
        return new ServiceCollection().AddLogging().AddAntiforgery().AddAdminDataProtection(configuration).BuildServiceProvider();
    }

    private static async Task MigrateAsync(string connectionString)
    {
        using var host = Host.CreateDefaultBuilder([])
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.Sources.Clear();
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["GenerateMigrations"] = "true",
                    ["ApplyMigrations"] = "true",
                    ["ConnectionStrings:PostgreSqlConnectionString"] = connectionString
                });
            })
            .ConfigureServices((context, services) =>
                services.AddAdminDataProtectionPersistence(context.Configuration))
            .Build();
        await host.RunMigrationsAsync<AdminDataProtectionDbContext>();
    }
}
