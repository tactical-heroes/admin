using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Observability.WebAssembly;

using TacticalHeroes.Admin.Client.App.Composition;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.AddObservability();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddTacticalHeroesAdminClient(
    builder.Configuration,
    baseAddressOverride: new Uri(builder.HostEnvironment.BaseAddress));

var host = builder.Build();
host.UseObservability();
host.Services.GetRequiredService<IStartupValidator>().Validate();

await host.RunAsync();
