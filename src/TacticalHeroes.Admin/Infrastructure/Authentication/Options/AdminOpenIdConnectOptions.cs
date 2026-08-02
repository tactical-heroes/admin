namespace TacticalHeroes.Admin.Infrastructure.Authentication.Options;

internal sealed class AdminOpenIdConnectOptions
{
    public const string SectionName = "Authentication:OpenIdConnect";

    public string Authority { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string CallbackPath { get; init; } = "/oauth/callback";

    public string SignedOutCallbackPath { get; init; } = "/oauth/logout-callback";

    public bool RequireHttpsMetadata { get; init; } = true;
}
