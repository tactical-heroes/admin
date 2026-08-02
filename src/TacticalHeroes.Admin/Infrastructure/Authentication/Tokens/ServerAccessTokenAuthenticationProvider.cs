using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Net.Http.Headers;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Tokens;

internal sealed class ServerAccessTokenAuthenticationProvider(
    IHttpContextAccessor httpContextAccessor) : IAuthenticationProvider
{
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        var accessToken = await httpContext.GetTokenAsync(
            AuthenticationConstants.SessionScheme,
            OpenIdConnectParameterNames.AccessToken);

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            var authorization = new AuthenticationHeaderValue(
                AuthenticationConstants.BearerScheme,
                accessToken);
            request.Headers.TryAdd(HeaderNames.Authorization, authorization.ToString());
        }
    }
}
