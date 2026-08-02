using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options;
using TacticalHeroes.Admin.Modules.Identity;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Session;

internal sealed class SessionCookieOptionsSetup(
    IOptions<AdminSessionOptions> configuredOptions,
    CookieOidcRefresher refresher)
    : IConfigureNamedOptions<CookieAuthenticationOptions>
{
    public void Configure(CookieAuthenticationOptions options)
    {
        Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
    }

    public void Configure(string? name, CookieAuthenticationOptions options)
    {
        if (!string.Equals(
                name,
                AuthenticationConstants.SessionScheme,
                StringComparison.Ordinal))
        {
            return;
        }

        var settings = configuredOptions.Value;

        options.Cookie.Name = settings.CookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = IdentityRoutes.Login;
        options.AccessDeniedPath = IdentityRoutes.Login;
        options.ExpireTimeSpan = settings.Lifetime;
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = context =>
            refresher.ValidateOrRefreshCookieAsync(
                context,
                AuthenticationConstants.OpenIdConnectScheme);
    }
}
