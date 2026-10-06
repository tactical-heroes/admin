using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TacticalHeroes.Admin.Infrastructure.DataProtection;

public sealed class AdminDataProtectionDbContextFactory : IDesignTimeDbContextFactory<AdminDataProtectionDbContext>
{
    public AdminDataProtectionDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSqlConnectionString")
            ?? "Host=localhost;Port=5432;Database=tactical-heroes;Username=postgres";
        var options = new DbContextOptionsBuilder<AdminDataProtectionDbContext>();
        options.UseNpgsql(connectionString);
        return new AdminDataProtectionDbContext(options.Options);
    }
}
