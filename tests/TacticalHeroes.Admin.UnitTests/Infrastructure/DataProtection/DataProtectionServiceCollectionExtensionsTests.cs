using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TacticalHeroes.Admin.Infrastructure.DataProtection;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.DataProtection;

public sealed class DataProtectionServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "AddAdminDataProtectionPersistence should configure PostgreSQL when connection is loaded from json")]
    public void AddAdminDataProtectionPersistence_Should_ConfigurePostgreSql_When_ConnectionIsLoadedFromJson()
    {
        using var json = new MemoryStream(Encoding.UTF8.GetBytes("""
            {
              "ConnectionStrings": {
                "PostgreSqlConnectionString": "Host=localhost;Port=5432;Database=tactical-heroes;Username=postgres"
              }
            }
            """));
        var configuration = new ConfigurationBuilder().AddJsonStream(json).Build();
        using var services = new ServiceCollection()
            .AddAdminDataProtectionPersistence(configuration)
            .BuildServiceProvider();
        using var scope = services.CreateScope();

        var database = scope.ServiceProvider.GetRequiredService<AdminDataProtectionDbContext>();

        database.Database.GetConnectionString().ShouldBe(configuration.GetConnectionString("PostgreSqlConnectionString"));
        database.Database.ProviderName.ShouldBe("Npgsql.EntityFrameworkCore.PostgreSQL");
        database.DataProtectionKeys.EntityType.GetTableName().ShouldBe("data_protection_keys");
        database.DataProtectionKeys.EntityType.GetSchema().ShouldBe("admin");
    }

    [Fact(DisplayName = "AddAdminDataProtection should preserve local startup when persistence is disabled")]
    public void AddAdminDataProtection_Should_PreserveLocalStartup_When_PersistenceIsDisabled()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddAdminDataProtection(configuration);

        services.ShouldNotContain(service => service.ServiceType == typeof(AdminDataProtectionDbContext));
    }

    [Theory(DisplayName = "AddAdminDataProtection should reject missing database settings when persistence is enabled")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddAdminDataProtection_Should_RejectMissingDatabaseSettings_When_PersistenceIsEnabled(string? connectionString)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DataProtection:Enabled"] = "true",
            ["ConnectionStrings:PostgreSqlConnectionString"] = connectionString
        }).Build();

        var exception = Should.Throw<InvalidOperationException>(() => services.AddAdminDataProtection(configuration));

        exception.Message.ShouldBe("ConnectionStrings:PostgreSqlConnectionString is required for PostgreSQL key storage.");
    }
}
