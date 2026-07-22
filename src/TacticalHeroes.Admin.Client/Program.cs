using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TacticalHeroes.Admin.Client.Shared.Api;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTacticalHeroesAdminClient(
    _ => new Uri(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync();
