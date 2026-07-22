using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace TacticalHeroes.Admin.Infrastructure.Authentication;

internal static class CookieOidcServiceCollectionExtensions
{
    internal static IServiceCollection ConfigureCookieOidc(
        this IServiceCollection services,
        string cookieScheme,
        string oidcScheme)
    {
        services.AddSingleton<CookieOidcRefresher>();
        services
            .AddOptions<CookieAuthenticationOptions>(cookieScheme)
            .Configure<CookieOidcRefresher>((options, refresher) =>
            {
                options.Events.OnValidatePrincipal = context =>
                    refresher.ValidateOrRefreshCookieAsync(context, oidcScheme);
            });
        services
            .AddOptions<OpenIdConnectOptions>(oidcScheme)
            .Configure(options =>
            {
                options.Scope.Add(OpenIdConnectScope.OfflineAccess);
                options.SaveTokens = true;
            });

        return services;
    }
}
