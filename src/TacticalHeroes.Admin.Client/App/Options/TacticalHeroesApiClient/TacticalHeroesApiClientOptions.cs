namespace TacticalHeroes.Admin.Client.App.Options.TacticalHeroesApiClient;

public sealed class TacticalHeroesApiClientOptions
{
    public const string SectionName = "TacticalHeroesApi";

    public string BaseUrl { get; set; } = string.Empty;

    public TimeSpan Timeout { get; init; }
}
