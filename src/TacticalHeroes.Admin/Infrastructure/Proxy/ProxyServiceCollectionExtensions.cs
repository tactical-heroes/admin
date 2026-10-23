using System.Net;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using TacticalHeroes.Admin.Infrastructure.Authentication;
using TacticalHeroes.Admin.Infrastructure.Proxy.Configuration;
using TacticalHeroes.Admin.Infrastructure.Proxy.Transforms;
using TacticalHeroes.Admin.Modules.Identity;

using Yarp.ReverseProxy.Transforms;

namespace TacticalHeroes.Admin.Infrastructure.Proxy;

internal static class ProxyServiceCollectionExtensions
{
    private const string ReverseProxySectionName = "ReverseProxy";
    private const string AttachSessionAccessTokenMetadata = "AttachSessionAccessToken";
    private const string RewriteLoginRedirectMetadata = "RewriteLoginRedirect";

    public static IServiceCollection AddTacticalHeroesProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddReverseProxy()
            .LoadFromConfig(configuration.GetRequiredSection(ReverseProxySectionName))
            .AddConfigFilter<TacticalHeroesApiProxyConfigFilter>()
            .ConfigureHttpClient((_, handler) => handler.ActivityHeadersPropagator = null)
            .AddTransforms(context =>
            {
                if (context.Route.Metadata is not null &&
                    context.Route.Metadata.TryGetValue(
                        RewriteLoginRedirectMetadata,
                        out string? rewriteLoginRedirect) &&
                    string.Equals(rewriteLoginRedirect, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                {
                    context.AddResponseTransform(transformContext =>
                    {
                        var location = transformContext.ProxyResponse?.Headers.Location;
                        if (transformContext.ProxyResponse?.StatusCode == HttpStatusCode.Redirect &&
                            location is { IsAbsoluteUri: true } &&
                            (location.Scheme == Uri.UriSchemeHttp || location.Scheme == Uri.UriSchemeHttps) &&
                            string.Equals(location.AbsolutePath, IdentityRoutes.Login, StringComparison.Ordinal))
                        {
                            transformContext.HttpContext.Response.Headers.Location = UriHelper.BuildRelative(
                                transformContext.HttpContext.Request.PathBase,
                                new PathString(IdentityRoutes.Login),
                                new QueryString(location.Query));
                        }

                        return ValueTask.CompletedTask;
                    });
                }

                if (context.Route.Metadata is null ||
                    !context.Route.Metadata.TryGetValue(
                        AttachSessionAccessTokenMetadata,
                        out string? attachSessionAccessToken) ||
                    !string.Equals(
                        attachSessionAccessToken,
                        bool.TrueString,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                context.AddRequestTransform(async transformContext =>
                {
                    var accessToken = await transformContext.HttpContext.GetTokenAsync(
                        AuthenticationConstants.SessionScheme,
                        OpenIdConnectParameterNames.AccessToken);

                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        transformContext.ProxyRequest.Headers.Authorization =
                            new AuthenticationHeaderValue(
                                AuthenticationConstants.BearerScheme,
                                accessToken);
                    }
                });

                context.ResponseTransforms.Add(new PasswordChangeSessionRefreshTransform());
            });

        return services;
    }
}
