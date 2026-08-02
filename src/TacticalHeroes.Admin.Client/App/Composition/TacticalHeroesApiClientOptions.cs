namespace TacticalHeroes.Admin.Client.App.Composition;

public sealed class TacticalHeroesApiClientOptions
{
    internal const string SectionName = "TacticalHeroesApi";

    public TimeSpan Timeout { get; init; }
}
