using TacticalHeroes.Admin.Components;
using TacticalHeroes.Admin.Client.Shared.Api;
using TacticalHeroes.Admin.Infrastructure.Api;
using TacticalHeroes.Admin.Infrastructure.Proxy;

var builder = WebApplication.CreateBuilder(args);
var apiBaseUri = builder.Configuration.GetTacticalHeroesApiBaseUri();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddTacticalHeroesAdminClient(_ => apiBaseUri);
builder.Services.AddTacticalHeroesProxy(apiBaseUri);
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHealthChecks("/health");
app.MapReverseProxy();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TacticalHeroes.Admin.Client._Imports).Assembly);

app.Run();
