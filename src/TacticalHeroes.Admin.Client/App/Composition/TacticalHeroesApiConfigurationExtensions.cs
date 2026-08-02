namespace TacticalHeroes.Admin.Client.App.Composition;

public static class TacticalHeroesApiConfigurationExtensions
{
    public static TacticalHeroesApiClientOptions GetTacticalHeroesApiClientOptions(
        this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        TacticalHeroesApiClientOptions options = configuration
            .GetRequiredSection(TacticalHeroesApiClientOptions.SectionName)
            .Get<TacticalHeroesApiClientOptions>() ?? new TacticalHeroesApiClientOptions();

        if (options.Timeout <= TimeSpan.Zero ||
            options.Timeout.TotalMilliseconds > int.MaxValue)
        {
            throw new InvalidOperationException(
                $"Configuration value '{TacticalHeroesApiClientOptions.SectionName}:Timeout' " +
                "must be a positive duration supported by HttpClient.");
        }

        return options;
    }
}
