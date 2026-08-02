using Microsoft.Extensions.Options;

namespace TacticalHeroes.Admin.Client.App.Options.DependencyInjection;

internal static class TacticalHeroesApiClientOptionsServiceCollectionExtensions
{
    public static IServiceCollection AddTacticalHeroesApiClientOptions(
        this IServiceCollection services,
        IConfiguration configuration,
        Uri? baseAddressOverride)
    {
        services.AddSingleton<
            IValidateOptions<TacticalHeroesApiClientOptions>,
            TacticalHeroesApiClientOptionsValidator>();
        services
            .AddOptions<TacticalHeroesApiClientOptions>()
            .Bind(configuration.GetSection(TacticalHeroesApiClientOptions.SectionName))
            .ValidateOnStart();

        if (baseAddressOverride is null)
        {
            return services;
        }

        if (!baseAddressOverride.IsAbsoluteUri)
        {
            throw new ArgumentException(
                "The API base address override must be absolute.",
                nameof(baseAddressOverride));
        }

        services.PostConfigure<TacticalHeroesApiClientOptions>(options =>
            options.BaseUrl = baseAddressOverride.AbsoluteUri);

        return services;
    }

    public static TacticalHeroesApiClientOptions GetTacticalHeroesApiClientOptions(
        this IServiceProvider serviceProvider)
    {
        return serviceProvider
            .GetRequiredService<IOptions<TacticalHeroesApiClientOptions>>()
            .Value;
    }
}
