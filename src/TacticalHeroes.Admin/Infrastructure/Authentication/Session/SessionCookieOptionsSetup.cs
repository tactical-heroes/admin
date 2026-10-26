using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Observability;

using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options.Session;
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
        options.Events.OnCheckSlidingExpiration = context =>
        {
            if (context.Request.IsBrowserTelemetryRequest())
            {
                context.ShouldRenew = false;
            }

            return Task.CompletedTask;
        };
        options.Events.OnValidatePrincipal = async context =>
        {
            if (context.Request.IsBrowserTelemetryRequest())
            {
                return;
            }

            await refresher.ValidateOrRefreshCookieAsync(
                context,
                AuthenticationConstants.OpenIdConnectScheme,
                forceRefresh: context.Request.Path == IdentityRoutes.AuthenticationRefresh,
                validateAccessToken: IsPageRequest(context.Request));
            if (context.Principal is null)
            {
                await context.HttpContext.SignOutAsync(AuthenticationConstants.SessionScheme);
            }
        };
    }

    private static bool IsPageRequest(HttpRequest request) =>
        HttpMethods.IsGet(request.Method)
        && request.Path != AdminRoutes.Error
        && request.HttpContext.GetEndpoint()?.Metadata.GetMetadata<ComponentTypeMetadata>() is not null
        && request.GetTypedHeaders().Accept?.Any(value =>
            string.Equals(value.MediaType.Value, "text/html", StringComparison.OrdinalIgnoreCase)) == true;
}
