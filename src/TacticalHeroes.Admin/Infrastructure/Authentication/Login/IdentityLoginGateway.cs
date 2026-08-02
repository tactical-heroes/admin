namespace TacticalHeroes.Admin.Infrastructure.Authentication.Login;

internal sealed class IdentityLoginGateway(HttpClient httpClient)
{
    private const string SignInPath = "/api/v1/auth/login";

    internal Task<HttpResponseMessage> SignInAsync(
        string email,
        string password,
        string returnUrl,
        CancellationToken cancellationToken)
    {
        return httpClient.PostAsJsonAsync(
            SignInPath,
            new ApiLoginRequest(email, password, returnUrl),
            cancellationToken);
    }

    private sealed record ApiLoginRequest(
        string Email,
        string Password,
        string ReturnUrl);
}
