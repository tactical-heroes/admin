using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;

using TacticalHeroes.Admin.Client.App.Composition;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddTacticalHeroesAdminClient(
    builder.Configuration,
    baseAddressOverride: new Uri(builder.HostEnvironment.BaseAddress));

var host = builder.Build();
host.Services.GetRequiredService<IStartupValidator>().Validate();

await host.RunAsync();
