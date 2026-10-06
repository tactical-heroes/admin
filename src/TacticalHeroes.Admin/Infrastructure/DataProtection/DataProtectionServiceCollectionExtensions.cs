using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace TacticalHeroes.Admin.Infrastructure.DataProtection;

public static class DataProtectionServiceCollectionExtensions
{
    public static IServiceCollection AddAdminDataProtection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("DataProtection:Enabled"))
        {
            return services;
        }

        services.AddAdminDataProtectionPersistence(configuration);
        services.AddDataProtection()
            .SetApplicationName("TacticalHeroes.Admin")
            .PersistKeysToDbContext<AdminDataProtectionDbContext>();

        return services;
    }

    public static IServiceCollection AddAdminDataProtectionPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("PostgreSqlConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:PostgreSqlConnectionString is required for PostgreSQL key storage.");
        }

        return services.AddDbContext<AdminDataProtectionDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
}
