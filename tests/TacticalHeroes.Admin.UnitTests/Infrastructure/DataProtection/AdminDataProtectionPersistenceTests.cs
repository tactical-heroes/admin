using System.Security.Cryptography;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Npgsql;

using TacticalHeroes.Admin.Infrastructure.DataProtection;

using Testcontainers.PostgreSql;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.DataProtection;

public sealed class AdminDataProtectionPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("admin_keys")
        .WithUsername("admin")
        .WithPassword("synthetic-test-password")
        .Build();

    public ValueTask InitializeAsync() => new(_database.StartAsync(TestContext.Current.CancellationToken));

    public ValueTask DisposeAsync() => _database.DisposeAsync();

    [Fact(DisplayName = "ApplyAdminDataProtectionMigrationsAsync should preserve the schema when migration is applied twice")]
    public async Task ApplyAdminDataProtectionMigrationsAsync_Should_PreserveSchema_When_MigrationIsAppliedTwice()
    {
        await using var services = CreateServices(_database.GetConnectionString());
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AdminDataProtectionDbContext>();

        await database.ApplyAdminDataProtectionMigrationsAsync(TestContext.Current.CancellationToken);
        await database.ApplyAdminDataProtectionMigrationsAsync(TestContext.Current.CancellationToken);

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
            await database.ApplyAdminDataProtectionMigrationsAsync(TestContext.Current.CancellationToken);
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
        await using var command = new NpgsqlCommand("CREATE DATABASE production_keys", connection);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        string productionConnection = new NpgsqlConnectionStringBuilder(_database.GetConnectionString())
        {
            Database = "production_keys"
        }.ConnectionString;
        await using var development = CreateServices(_database.GetConnectionString());
        await using var production = CreateServices(productionConnection);
        await MigrateAsync(development);
        await MigrateAsync(production);
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
        await using (var original = CreateServices(_database.GetConnectionString()))
        {
            await MigrateAsync(original);
            var context = new DefaultHttpContext { RequestServices = original };
            tokens = original.GetRequiredService<IAntiforgery>().GetTokens(context);
            cookieName = original.GetRequiredService<IOptions<AntiforgeryOptions>>().Value.Cookie.Name!;
        }

        await using var replacement = CreateServices(_database.GetConnectionString());
        var request = new DefaultHttpContext { RequestServices = replacement };
        request.Request.Method = HttpMethods.Post;
        request.Request.Headers.Cookie = $"{cookieName}={tokens.CookieToken}";
        request.Request.Headers["RequestVerificationToken"] = tokens.RequestToken;

        var exception = await Record.ExceptionAsync(() => replacement.GetRequiredService<IAntiforgery>().ValidateRequestAsync(request));

        exception.ShouldBeNull();
    }

    private static ServiceProvider CreateServices(string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DataProtection:Enabled"] = "true",
            ["ConnectionStrings:DataProtection"] = connectionString
        }).Build();
        return new ServiceCollection().AddLogging().AddAntiforgery().AddAdminDataProtection(configuration).BuildServiceProvider();
    }

    private static async Task MigrateAsync(ServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AdminDataProtectionDbContext>();
        await database.ApplyAdminDataProtectionMigrationsAsync(TestContext.Current.CancellationToken);
    }
}
