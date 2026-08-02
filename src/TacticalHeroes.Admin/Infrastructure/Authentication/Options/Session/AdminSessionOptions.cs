namespace TacticalHeroes.Admin.Infrastructure.Authentication.Options.Session;

internal sealed class AdminSessionOptions
{
    public const string SectionName = "Authentication:Session";

    public string CookieName { get; init; } = string.Empty;

    public TimeSpan Lifetime { get; init; }
}
