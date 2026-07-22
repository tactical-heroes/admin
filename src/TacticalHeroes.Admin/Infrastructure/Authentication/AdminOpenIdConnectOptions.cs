namespace TacticalHeroes.Admin.Infrastructure.Authentication;

internal sealed class AdminOpenIdConnectOptions
{
    internal const string SectionName = "Authentication:OpenIdConnect";

    public string Authority { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string CallbackPath { get; init; } = "/oauth/callback";

    public string SignedOutCallbackPath { get; init; } = "/oauth/logout-callback";

    public bool RequireHttpsMetadata { get; init; } = true;

    internal void Validate()
    {
        if (!Uri.TryCreate(Authority, UriKind.Absolute, out var authority) ||
            !IsHttpScheme(authority))
        {
            throw new InvalidOperationException(
                $"{SectionName}:Authority must be an absolute HTTP or HTTPS URI.");
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new InvalidOperationException($"{SectionName}:ClientId is required.");
        }

        ValidatePath(CallbackPath, nameof(CallbackPath));
        ValidatePath(SignedOutCallbackPath, nameof(SignedOutCallbackPath));
    }

    private static void ValidatePath(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !value.StartsWith('/') ||
            value.StartsWith("//", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{SectionName}:{name} must be an absolute application path.");
        }
    }

    private static bool IsHttpScheme(Uri uri)
    {
        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
