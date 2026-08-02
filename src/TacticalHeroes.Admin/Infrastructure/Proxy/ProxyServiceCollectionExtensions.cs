using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using TacticalHeroes.Admin.Infrastructure.Authentication;
using TacticalHeroes.Admin.Infrastructure.Proxy.Configuration;

using Yarp.ReverseProxy.Transforms;

namespace TacticalHeroes.Admin.Infrastructure.Proxy;

internal static class ProxyServiceCollectionExtensions
{
    private const string ReverseProxySectionName = "ReverseProxy";
    private const string AttachSessionAccessTokenMetadata = "AttachSessionAccessToken";

    public static IServiceCollection AddTacticalHeroesProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddReverseProxy()
            .LoadFromConfig(configuration.GetRequiredSection(ReverseProxySectionName))
            .AddConfigFilter<TacticalHeroesApiProxyConfigFilter>()
            .AddTransforms(context =>
            {
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
            });

        return services;
    }
}
