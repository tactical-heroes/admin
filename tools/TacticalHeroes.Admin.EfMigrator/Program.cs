using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PANiXiDA.Core.Ef.Migrator;

using TacticalHeroes.Admin.Infrastructure.DataProtection;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, configuration) =>
    {
        configuration
            .AddJsonFile("appsettings.Migrator.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
        services.AddAdminDataProtectionPersistence(context.Configuration))
    .Build();

if (host.Services.GetRequiredService<IConfiguration>().GetValue("ApplyMigrations", true))
{
    await using var scope = host.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<AdminDataProtectionDbContext>();
    await database.GetService<IHistoryRepository>().CreateIfNotExistsAsync();
}

await host.RunMigrationsAsync<AdminDataProtectionDbContext>();
