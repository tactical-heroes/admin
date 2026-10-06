using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TacticalHeroes.Admin.Infrastructure.DataProtection;

public sealed class AdminDataProtectionDbContext(DbContextOptions<AdminDataProtectionDbContext> options)
    : DbContext(options), IDataProtectionKeyContext
{
    internal const string Schema = "admin";

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.UseNpgsql(postgres =>
            postgres.MigrationsHistoryTable("__ef_migrations_history", Schema));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
    }
}
