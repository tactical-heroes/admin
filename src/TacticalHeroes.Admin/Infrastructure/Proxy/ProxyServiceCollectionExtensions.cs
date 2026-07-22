using Microsoft.AspNetCore.Authentication;
using TacticalHeroes.Admin.Infrastructure.Authentication;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace TacticalHeroes.Admin.Infrastructure.Proxy;

internal static class ProxyServiceCollectionExtensions
{
    private const string ApiClusterId = "tactical-heroes-api";
    private const string ProtectedApiRouteId = "tactical-heroes-protected-api";
    private const string OpenIdConnectRouteId = "tactical-heroes-openid-connect";

    public static IServiceCollection AddTacticalHeroesProxy(
        this IServiceCollection services,
        Uri apiBaseUri)
    {
        string[] anonymousAuthPaths =
        [
            "/api/v1/auth/login",
            "/api/v1/auth/register",
            "/api/v1/auth/confirm-email",
            "/api/v1/auth/resend-confirmation-email",
            "/api/v1/auth/forgot-password",
            "/api/v1/auth/reset-password",
        ];
        var routes = anonymousAuthPaths
            .Select((path, index) => new RouteConfig
            {
                RouteId = $"tactical-heroes-anonymous-auth-{index}",
                ClusterId = ApiClusterId,
                Match = new RouteMatch { Path = path },
            })
            .ToList();
        routes.Add(
            new RouteConfig
            {
                RouteId = ProtectedApiRouteId,
                ClusterId = ApiClusterId,
                AuthorizationPolicy = AuthenticationConstants.ApiAuthorizationPolicy,
                Match = new RouteMatch
                {
                    Path = "/api/{**catch-all}",
                },
            });
        routes.Add(
            new RouteConfig
            {
                RouteId = OpenIdConnectRouteId,
                ClusterId = ApiClusterId,
                Match = new RouteMatch
                {
                    Path = "/connect/{**catch-all}",
                },
            });

        var clusters = new[]
        {
            new ClusterConfig
            {
                ClusterId = ApiClusterId,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["primary"] = new DestinationConfig
                    {
                        Address = $"{apiBaseUri.AbsoluteUri.TrimEnd('/')}/",
                    },
                },
            },
        };

        services
            .AddReverseProxy()
            .LoadFromMemory(routes, clusters)
            .AddTransforms(context =>
            {
                if (!string.Equals(
                        context.Route.RouteId,
                        ProtectedApiRouteId,
                        StringComparison.Ordinal))
                {
                    return;
                }

                context.AddRequestTransform(async transformContext =>
                {
                    var accessToken = await transformContext.HttpContext.GetTokenAsync(
                        AuthenticationConstants.SessionScheme,
                        "access_token");

                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        transformContext.ProxyRequest.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue(
                                "Bearer",
                                accessToken);
                    }
                });
            });

        return services;
    }
}
