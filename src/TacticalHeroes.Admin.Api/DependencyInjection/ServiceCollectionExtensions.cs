using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Extensions.DependencyInjection;
using TacticalHeroes.Admin.Api.Generated;

namespace TacticalHeroes.Admin.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private const string ApiHttpClientName = "TacticalHeroesApi";

    public static IServiceCollection AddTacticalHeroesApiClient(
        this IServiceCollection services,
        Func<IServiceProvider, Uri> baseAddressFactory,
        Func<IServiceProvider, IAuthenticationProvider>? authenticationProviderFactory = null)
    {
        ArgumentNullException.ThrowIfNull(baseAddressFactory);

        services.AddHttpClient(
            ApiHttpClientName,
            (serviceProvider, httpClient) =>
            {
                Uri baseAddress = baseAddressFactory(serviceProvider);

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
                HttpClient httpClient = serviceProvider
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient(ApiHttpClientName);
                IAuthenticationProvider authenticationProvider =
                    authenticationProviderFactory?.Invoke(serviceProvider) ??
                    new AnonymousAuthenticationProvider();
                HttpClientRequestAdapter requestAdapter = new(
                    authenticationProvider,
                    httpClient: httpClient)
                {
                    BaseUrl = httpClient.BaseAddress!.AbsoluteUri.TrimEnd('/'),
                };

                return new TacticalHeroesApiClient(requestAdapter);
            });

        return services;
    }
}
