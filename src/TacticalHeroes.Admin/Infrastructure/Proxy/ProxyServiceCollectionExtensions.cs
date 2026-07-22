using Yarp.ReverseProxy.Configuration;

namespace TacticalHeroes.Admin.Infrastructure.Proxy;

internal static class ProxyServiceCollectionExtensions
{
    private const string ApiClusterId = "tactical-heroes-api";

    public static IServiceCollection AddTacticalHeroesProxy(
        this IServiceCollection services,
        Uri apiBaseUri)
    {
        var routes = new[]
        {
            new RouteConfig
            {
                RouteId = "tactical-heroes-api-route",
                ClusterId = ApiClusterId,
                Match = new RouteMatch
                {
                    Path = "/api/{**catch-all}",
                },
            },
        };

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

        services.AddReverseProxy().LoadFromMemory(routes, clusters);

        return services;
    }
}
