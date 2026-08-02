using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;

internal sealed class OpenIdConnectEndpointResolver(
    IOptionsMonitor<OpenIdConnectOptions> optionsMonitor)
{
    internal async Task<string> GetAuthorizationPathAsync(
        CancellationToken cancellationToken)
    {
        var options = optionsMonitor.Get(AuthenticationConstants.OpenIdConnectScheme);
        var configurationManager = options.ConfigurationManager ??
            throw new InvalidOperationException(
                "The OIDC configuration manager is missing.");
        var configuration = await configurationManager.GetConfigurationAsync(cancellationToken);

        if (!Uri.TryCreate(
                configuration.AuthorizationEndpoint,
                UriKind.Absolute,
                out var endpoint))
        {
            throw new InvalidOperationException(
                "The OIDC authorization endpoint is missing or invalid.");
        }

        return endpoint.AbsolutePath;
    }
}
