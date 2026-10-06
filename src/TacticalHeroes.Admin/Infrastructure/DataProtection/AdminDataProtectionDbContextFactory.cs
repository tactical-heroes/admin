using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TacticalHeroes.Admin.Infrastructure.DataProtection;

public sealed class AdminDataProtectionDbContextFactory : IDesignTimeDbContextFactory<AdminDataProtectionDbContext>
{
    public AdminDataProtectionDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DataProtection")
            ?? "Host=localhost;Database=tactical_heroes_dev;Username=postgres";
        var options = new DbContextOptionsBuilder<AdminDataProtectionDbContext>();
        DataProtectionServiceCollectionExtensions.ConfigureDatabase(options, connectionString);
        return new AdminDataProtectionDbContext(options.Options);
    }
}
