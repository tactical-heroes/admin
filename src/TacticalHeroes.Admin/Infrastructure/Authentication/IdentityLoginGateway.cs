using System.Net;
using Microsoft.Net.Http.Headers;

namespace TacticalHeroes.Admin.Infrastructure.Authentication;

internal sealed class IdentityLoginGateway(HttpClient httpClient)
{
    internal async Task<IdentityLoginResult> SignInAsync(
        string email,
        string password,
        string returnUrl,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/v1/auth/login",
            new ApiLoginRequest(email, password, returnUrl),
            cancellationToken);

        var status = response.StatusCode switch
        {
            HttpStatusCode.Redirect => IdentityLoginStatus.Succeeded,
            HttpStatusCode.BadRequest => IdentityLoginStatus.InvalidRequest,
            HttpStatusCode.Unauthorized => IdentityLoginStatus.InvalidCredentials,
            HttpStatusCode.Forbidden => IdentityLoginStatus.Forbidden,
            _ => IdentityLoginStatus.Unavailable,
        };
        var cookies = response.Headers.TryGetValues(HeaderNames.SetCookie, out var values)
            ? values.ToArray()
            : [];

        if (status == IdentityLoginStatus.Succeeded && cookies.Length == 0)
        {
            status = IdentityLoginStatus.Unavailable;
        }

        return new IdentityLoginResult(status, cookies);
    }

    private sealed record ApiLoginRequest(
        string Email,
        string Password,
        string ReturnUrl);
}

internal sealed record IdentityLoginResult(
    IdentityLoginStatus Status,
    IReadOnlyCollection<string> SetCookieHeaders);

internal enum IdentityLoginStatus
{
    Succeeded,
    InvalidRequest,
    InvalidCredentials,
    Forbidden,
    Unavailable,
}
