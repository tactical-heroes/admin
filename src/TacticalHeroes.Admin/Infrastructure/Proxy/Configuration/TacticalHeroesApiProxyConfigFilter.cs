using Microsoft.Extensions.Options;

using TacticalHeroes.Admin.Client.App.Options.TacticalHeroesApiClient;

using Yarp.ReverseProxy.Configuration;

namespace TacticalHeroes.Admin.Infrastructure.Proxy.Configuration;

internal sealed class TacticalHeroesApiProxyConfigFilter(
    IOptionsMonitor<TacticalHeroesApiClientOptions> apiOptions) : IProxyConfigFilter
{
    private const string BaseUrlPlaceholder = "{TacticalHeroesApi:BaseUrl}";

    public ValueTask<ClusterConfig> ConfigureClusterAsync(
        ClusterConfig cluster,
        CancellationToken cancel)
    {
        if (cluster.Destinations is null ||
            !cluster.Destinations.Values.Any(destination =>
                string.Equals(
                    destination.Address,
                    BaseUrlPlaceholder,
                    StringComparison.Ordinal)))
        {
            return ValueTask.FromResult(cluster);
        }

        string baseUrl = $"{apiOptions.CurrentValue.BaseUrl.TrimEnd('/')}/";
        Dictionary<string, DestinationConfig> destinations = cluster.Destinations.ToDictionary(
            destination => destination.Key,
            destination => string.Equals(
                destination.Value.Address,
                BaseUrlPlaceholder,
                StringComparison.Ordinal)
                    ? destination.Value with { Address = baseUrl }
                    : destination.Value,
            StringComparer.OrdinalIgnoreCase);

        return ValueTask.FromResult(cluster with { Destinations = destinations });
    }

    public ValueTask<RouteConfig> ConfigureRouteAsync(
        RouteConfig route,
        ClusterConfig? cluster,
        CancellationToken cancel)
    {
        return ValueTask.FromResult(route);
    }
}
