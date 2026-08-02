using Microsoft.Extensions.Options;

using TacticalHeroes.Admin.Infrastructure.Authentication.Options.IdentityLogin;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Login;

internal sealed class IdentityLoginGateway(
    HttpClient httpClient,
    IOptionsMonitor<IdentityLoginRouteOptions> routeOptions)
{
    internal Task<HttpResponseMessage> SignInAsync(
        string email,
        string password,
        string returnUrl,
        CancellationToken cancellationToken)
    {
        return httpClient.PostAsJsonAsync(
            routeOptions.CurrentValue.Path,
            new ApiLoginRequest(email, password, returnUrl),
            cancellationToken);
    }

    private sealed record ApiLoginRequest(
        string Email,
        string Password,
        string ReturnUrl);
}
