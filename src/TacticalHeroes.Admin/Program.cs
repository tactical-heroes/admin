using TacticalHeroes.Admin.Client.App.Composition;
using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Components;
using TacticalHeroes.Admin.Infrastructure.Authentication;
using TacticalHeroes.Admin.Infrastructure.Proxy;
using TacticalHeroes.Admin.Modules.Compendium;
using TacticalHeroes.Admin.Modules.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTacticalHeroesAdminClient(
    builder.Configuration,
    authenticationProviderFactory: services =>
        services.GetRequiredService<ServerAccessTokenAuthenticationProvider>());
builder.Services.AddAdminAuthentication(builder.Configuration);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();
builder.Services.AddTacticalHeroesProxy(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler(AdminRoutes.Error, createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(AdminRoutes.NotFound, createScopeForStatusCodePages: true);
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

await app.RunAsync();
