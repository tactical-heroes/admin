using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using MudBlazor.Services;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Client.Entities.Authentication.Api;
using TacticalHeroes.Admin.Client.Entities.Roles.Api;
using TacticalHeroes.Admin.Client.Entities.Users.Api;

namespace TacticalHeroes.Admin.Client.Shared.Api;

public static class ClientServiceCollectionExtensions
{
    private const string ApiHttpClientName = "TacticalHeroesApi";

    public static IServiceCollection AddTacticalHeroesAdminClient(
        this IServiceCollection services,
        Func<IServiceProvider, Uri> baseAddressFactory,
        Func<IServiceProvider, IAuthenticationProvider>? authenticationProviderFactory = null)
    {
        ArgumentNullException.ThrowIfNull(baseAddressFactory);

        services.AddMudServices();

        services.AddHttpClient(
            ApiHttpClientName,
            (serviceProvider, httpClient) =>
            {
                var baseAddress = baseAddressFactory(serviceProvider);

                if (!baseAddress.IsAbsoluteUri)
                {
                    throw new InvalidOperationException(
                        "The Tactical Heroes API base address must be absolute.");
                }

                httpClient.BaseAddress = new Uri(
                    $"{baseAddress.AbsoluteUri.TrimEnd('/')}/",
                    UriKind.Absolute);
                httpClient.Timeout = TimeSpan.FromSeconds(30);
            });

        services.AddScoped(
            serviceProvider =>
            {
                var httpClient = serviceProvider
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient(ApiHttpClientName);
                var authenticationProvider = authenticationProviderFactory?.Invoke(serviceProvider) ??
                    new AnonymousAuthenticationProvider();
                var requestAdapter = new HttpClientRequestAdapter(
                    authenticationProvider,
                    httpClient: httpClient)
                {
                    BaseUrl = httpClient.BaseAddress!.AbsoluteUri.TrimEnd('/'),
                };

                return new TacticalHeroesApiClient(requestAdapter);
            });

        services.AddScoped<RolesApi>();
        services.AddScoped<UsersApi>();
        services.AddScoped<AuthenticationApi>();

        return services;
    }
}
