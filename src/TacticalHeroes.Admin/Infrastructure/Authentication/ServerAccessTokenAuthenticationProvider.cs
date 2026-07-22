using Microsoft.AspNetCore.Authentication;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace TacticalHeroes.Admin.Infrastructure.Authentication;

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
            "access_token");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.TryAdd("Authorization", $"Bearer {accessToken}");
        }
    }
}
