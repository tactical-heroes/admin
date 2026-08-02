namespace TacticalHeroes.Admin.Infrastructure.Authentication.Options;

internal sealed class AdminOpenIdConnectOptions
{
    public const string SectionName = "Authentication:OpenIdConnect";

    public string Authority { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string CallbackPath { get; init; } = string.Empty;

    public string SignedOutCallbackPath { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; } = true;

    public TimeSpan RefreshBeforeExpiration { get; init; }

    public string NameClaimType { get; init; } = string.Empty;

    public string RoleClaimType { get; init; } = string.Empty;

    public List<string> Scopes { get; init; } = [];
}
