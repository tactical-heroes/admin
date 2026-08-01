using TacticalHeroes.Admin.Components;
using TacticalHeroes.Admin.Client.App.Composition;
using TacticalHeroes.Admin.Infrastructure.Api;
using TacticalHeroes.Admin.Infrastructure.Authentication;
using TacticalHeroes.Admin.Infrastructure.Proxy;
using TacticalHeroes.Admin.Modules.Compendium;
using TacticalHeroes.Admin.Modules.Identity;

var builder = WebApplication.CreateBuilder(args);
var apiBaseUri = builder.Configuration.GetTacticalHeroesApiBaseUri();

builder.Services.AddAdminAuthentication(builder.Configuration, apiBaseUri);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();
builder.Services.AddTacticalHeroesAdminClient(
    _ => apiBaseUri,
    services => services.GetRequiredService<ServerAccessTokenAuthenticationProvider>());
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
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHealthChecks("/health");
app.MapReverseProxy();
app.MapAdminAuthentication();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(TacticalHeroes.Admin.Client._Imports).Assembly,
        CompendiumAdminModule.Assembly,
        IdentityAdminModule.Assembly);

app.Run();
