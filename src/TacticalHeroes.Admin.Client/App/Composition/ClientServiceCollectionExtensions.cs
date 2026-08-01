using Microsoft.Kiota.Abstractions.Authentication;
using MudBlazor.Services;
using TacticalHeroes.Admin.Api.DependencyInjection;

namespace TacticalHeroes.Admin.Client.App.Composition;

public static class ClientServiceCollectionExtensions
{
    public static IServiceCollection AddTacticalHeroesAdminClient(
        this IServiceCollection services,
        Func<IServiceProvider, Uri> baseAddressFactory,
        Func<IServiceProvider, IAuthenticationProvider>? authenticationProviderFactory = null)
    {
        services.AddMudServices();
        services.AddTacticalHeroesApiClient(
            baseAddressFactory,
            authenticationProviderFactory);
        services.AddAdminModules();

        return services;
    }
}
