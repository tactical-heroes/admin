using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TacticalHeroes.Admin.Client.App.Composition;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddTacticalHeroesAdminClient(
    _ => new Uri(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync();
