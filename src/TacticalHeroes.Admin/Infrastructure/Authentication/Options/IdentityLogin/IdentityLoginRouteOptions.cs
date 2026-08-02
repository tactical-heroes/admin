namespace TacticalHeroes.Admin.Infrastructure.Authentication.Options.IdentityLogin;

internal sealed class IdentityLoginRouteOptions
{
    public const string SectionName =
        "ReverseProxy:Routes:tactical-heroes-auth-login:Match";

    public string Path { get; init; } = string.Empty;
}
