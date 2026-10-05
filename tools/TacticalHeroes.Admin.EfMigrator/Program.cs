using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TacticalHeroes.Admin.Infrastructure.DataProtection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddAdminDataProtectionPersistence(builder.Configuration);
using var host = builder.Build();
await using var scope = host.Services.CreateAsyncScope();
var database = scope.ServiceProvider.GetRequiredService<AdminDataProtectionDbContext>();
await database.ApplyAdminDataProtectionMigrationsAsync(CancellationToken.None);
