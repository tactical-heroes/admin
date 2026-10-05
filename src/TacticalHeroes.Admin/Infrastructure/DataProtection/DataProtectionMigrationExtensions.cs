using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TacticalHeroes.Admin.Infrastructure.DataProtection;

public static class DataProtectionMigrationExtensions
{
    public static async Task ApplyAdminDataProtectionMigrationsAsync(
        this AdminDataProtectionDbContext database,
        CancellationToken cancellationToken)
    {
        await database.GetService<IHistoryRepository>().CreateIfNotExistsAsync(cancellationToken);
        await database.Database.MigrateAsync(cancellationToken);
    }
}
