using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

using TacticalHeroes.Admin.Api.Authentication;
using TacticalHeroes.Admin.Api.Generated;

namespace TacticalHeroes.Admin.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private const string ApiHttpClientName = "TacticalHeroesApi";

    public static IServiceCollection AddTacticalHeroesApiClient(
        this IServiceCollection services,
        Func<IServiceProvider, Uri> baseAddressFactory,
        Func<IServiceProvider, TimeSpan> requestTimeoutFactory,
        Func<IServiceProvider, IAuthenticationProvider>? authenticationProviderFactory = null)
    {
        ArgumentNullException.ThrowIfNull(baseAddressFactory);
        ArgumentNullException.ThrowIfNull(requestTimeoutFactory);

        services
            .AddHttpClient(
                ApiHttpClientName,
                (serviceProvider, httpClient) =>
                {
                    Uri baseAddress = baseAddressFactory(serviceProvider);

                    if (!baseAddress.IsAbsoluteUri)
                    {
                        throw new InvalidOperationException(
                            "The API base address must be absolute.");
                    }

                    httpClient.BaseAddress = new Uri(
                        $"{baseAddress.AbsoluteUri.TrimEnd('/')}/",
                        UriKind.Absolute);
                    httpClient.Timeout = requestTimeoutFactory(serviceProvider);
                })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.DisableForUnsafeHttpMethods();
            });

        services.AddScoped(
            serviceProvider =>
            {
                HttpClient httpClient = serviceProvider.GetRequiredKeyedService<HttpClient>(ApiHttpClientName);
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

        services.AddScoped<SessionRefreshState>();
        services.AddKeyedScoped<HttpClient>(ApiHttpClientName, (serviceProvider, _) =>
        {
            var handler = new SessionRefreshHandler(serviceProvider.GetRequiredService<SessionRefreshState>())
            {
                InnerHandler = serviceProvider.GetRequiredService<IHttpMessageHandlerFactory>()
                    .CreateHandler(ApiHttpClientName)
            };
            var client = new HttpClient(handler);
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>()
                .Get(ApiHttpClientName);
            foreach (var configure in options.HttpClientActions)
            {
                configure(client);
            }

            return client;
        });

        return services;
    }
}
