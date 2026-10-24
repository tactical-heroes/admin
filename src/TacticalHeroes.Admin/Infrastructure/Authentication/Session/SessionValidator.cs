using System.Net;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Session;

internal sealed class SessionValidator(IOptionsMonitor<OpenIdConnectOptions> optionsMonitor)
{
    internal async Task<HttpStatusCode> ValidateAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return HttpStatusCode.Unauthorized;
        }

        var accessToken = await context.GetTokenAsync(
            AuthenticationConstants.SessionScheme, OpenIdConnectParameterNames.AccessToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await context.SignOutAsync(AuthenticationConstants.SessionScheme);
            return HttpStatusCode.Unauthorized;
        }

        var options = optionsMonitor.Get(AuthenticationConstants.OpenIdConnectScheme);
        try
        {
            var configurationManager = options.ConfigurationManager ??
                throw new InvalidOperationException("The OIDC configuration manager is missing.");
            var configuration = await configurationManager.GetConfigurationAsync(cancellationToken);
            var endpoint = configuration.UserInfoEndpoint ??
                throw new InvalidOperationException("The OIDC user info endpoint is missing.");
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationConstants.BearerScheme, accessToken);
            using var response = await options.Backchannel.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HttpStatusCode.NoContent;
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                await context.SignOutAsync(AuthenticationConstants.SessionScheme);
                return HttpStatusCode.Unauthorized;
            }

            return HttpStatusCode.ServiceUnavailable;
        }
        catch (HttpRequestException)
        {
            return HttpStatusCode.ServiceUnavailable;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HttpStatusCode.ServiceUnavailable;
        }
    }
}
