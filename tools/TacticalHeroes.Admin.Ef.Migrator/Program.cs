using Microsoft.Extensions.Configuration;
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

await host.RunMigrationsAsync<AdminDataProtectionDbContext>();
