using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

using TacticalHeroes.Admin.Client.App.Options.TacticalHeroesApiClient;
using TacticalHeroes.Admin.Infrastructure.Authentication.Login;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options.DependencyInjection;
using TacticalHeroes.Admin.Infrastructure.Authentication.Session;
using TacticalHeroes.Admin.Infrastructure.Authentication.Tokens;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.DependencyInjection;

internal static class AuthenticationServiceCollectionExtensions
{
    internal static IServiceCollection AddAdminAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAdminAuthenticationOptions(configuration);
        services.AddSingleton<CookieOidcRefresher>();
        services.AddSingleton<OpenIdConnectEndpointResolver>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthenticationConstants.SessionScheme;
                options.DefaultSignInScheme = AuthenticationConstants.SessionScheme;
                options.DefaultChallengeScheme = AuthenticationConstants.OpenIdConnectScheme;
            })
            .AddCookie(AuthenticationConstants.SessionScheme, static _ => { })
            .AddOpenIdConnect(AuthenticationConstants.OpenIdConnectScheme, static _ => { });

        services.AddSingleton<
            IConfigureOptions<CookieAuthenticationOptions>,
            SessionCookieOptionsSetup>();
        services.AddSingleton<
            IConfigureOptions<OpenIdConnectOptions>,
            AdminOpenIdConnectOptionsSetup>();

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                AuthenticationConstants.ApiAuthorizationPolicy,
                policy => policy.RequireAuthenticatedUser());
        services.AddCascadingAuthenticationState();
        services.AddHttpContextAccessor();
        services.AddScoped<ServerAccessTokenAuthenticationProvider>();
        services.AddHttpClient<IdentityLoginGateway>((serviceProvider, httpClient) =>
            {
                var apiOptions = serviceProvider
                    .GetRequiredService<IOptions<TacticalHeroesApiClientOptions>>()
                    .Value;
                httpClient.BaseAddress = new Uri(
                    $"{apiOptions.BaseUrl.TrimEnd('/')}/",
                    UriKind.Absolute);
                httpClient.Timeout = apiOptions.Timeout;
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false,
            });

        return services;
    }
}
