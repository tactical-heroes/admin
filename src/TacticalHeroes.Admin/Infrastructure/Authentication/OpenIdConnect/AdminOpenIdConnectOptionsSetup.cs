using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options.OpenIdConnect;
using TacticalHeroes.Admin.Modules.Identity;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;

internal sealed class AdminOpenIdConnectOptionsSetup(
    IOptions<AdminOpenIdConnectOptions> configuredOptions)
    : IConfigureNamedOptions<OpenIdConnectOptions>
{
    public void Configure(OpenIdConnectOptions options)
    {
        Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
    }

    public void Configure(string? name, OpenIdConnectOptions options)
    {
        if (!string.Equals(
                name,
                AuthenticationConstants.OpenIdConnectScheme,
                StringComparison.Ordinal))
        {
            return;
        }

        var settings = configuredOptions.Value;

        options.SignInScheme = AuthenticationConstants.SessionScheme;
        options.Authority = settings.Authority.TrimEnd('/');
        options.ClientId = settings.ClientId;
        options.CallbackPath = settings.CallbackPath;
        options.SignedOutCallbackPath = settings.SignedOutCallbackPath;
        options.SignedOutRedirectUri = AdminRoutes.Home;
        options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Require;
        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
        options.TokenValidationParameters.NameClaimType = settings.NameClaimType;
        options.TokenValidationParameters.RoleClaimType = settings.RoleClaimType;
        options.Scope.Clear();

        foreach (string scope in settings.Scopes)
        {
            options.Scope.Add(scope);
        }

        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                context.ProtocolMessage.IssuerAddress = BuildBffEndpoint(
                    context.Request,
                    context.ProtocolMessage.IssuerAddress);
                return Task.CompletedTask;
            },
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                context.ProtocolMessage.IssuerAddress = BuildBffEndpoint(
                    context.Request,
                    context.ProtocolMessage.IssuerAddress);
                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                context.HandleResponse();
                context.Response.Redirect(
                    IdentityRoutes.LoginPage(error: AuthenticationError.OAuth));
                return Task.CompletedTask;
            },
        };
    }

    private static string BuildBffEndpoint(HttpRequest request, string? endpointAddress)
    {
        if (!Uri.TryCreate(endpointAddress, UriKind.Absolute, out var endpoint))
        {
            throw new InvalidOperationException(
                "The OIDC endpoint address is missing or invalid.");
        }

        return UriHelper.BuildAbsolute(
            request.Scheme,
            request.Host,
            request.PathBase,
            new PathString(endpoint.AbsolutePath),
            new QueryString(endpoint.Query));
    }
}
