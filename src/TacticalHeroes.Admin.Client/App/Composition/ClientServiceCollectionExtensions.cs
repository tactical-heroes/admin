using Microsoft.Kiota.Abstractions.Authentication;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Client.App.Options.DependencyInjection;

namespace TacticalHeroes.Admin.Client.App.Composition;

public static class ClientServiceCollectionExtensions
{
    public static IServiceCollection AddTacticalHeroesAdminClient(
        this IServiceCollection services,
        IConfiguration configuration,
        Uri? baseAddressOverride = null,
        Func<IServiceProvider, IAuthenticationProvider>? authenticationProviderFactory = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddTacticalHeroesApiClientOptions(configuration, baseAddressOverride);
        services.AddMudServices();
        services.AddTacticalHeroesApiClient(
            static serviceProvider => new Uri(
                serviceProvider.GetTacticalHeroesApiClientOptions().BaseUrl,
                UriKind.Absolute),
            static serviceProvider => serviceProvider
                .GetTacticalHeroesApiClientOptions()
                .Timeout,
            authenticationProviderFactory);
        services.AddAdminModules();

        return services;
    }
}
