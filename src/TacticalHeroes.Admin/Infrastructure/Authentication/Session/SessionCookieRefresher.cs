using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Session;

internal sealed class SessionCookieRefresher(
    CookieOidcRefresher oidcRefresher,
    IOptionsMonitor<CookieAuthenticationOptions> options)
{
    internal async Task RefreshAsync(HttpContext httpContext)
    {
        var authentication = await httpContext.AuthenticateAsync(AuthenticationConstants.SessionScheme);
        if (authentication.Ticket is null)
        {
            return;
        }

        var context = new CookieValidatePrincipalContext(
            httpContext,
            new AuthenticationScheme(AuthenticationConstants.SessionScheme, null, typeof(CookieAuthenticationHandler)),
            options.Get(AuthenticationConstants.SessionScheme),
            authentication.Ticket);
        await oidcRefresher.ValidateOrRefreshCookieAsync(
            context, AuthenticationConstants.OpenIdConnectScheme, forceRefresh: true);
        if (context.Principal is null)
        {
            await httpContext.SignOutAsync(AuthenticationConstants.SessionScheme);
            return;
        }

        await httpContext.SignInAsync(AuthenticationConstants.SessionScheme, context.Principal, context.Properties);
    }
}
