using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Client.App.Options;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options;
using TacticalHeroes.Admin.Modules.Identity;

namespace TacticalHeroes.Admin.Infrastructure.Authentication;

internal static class AuthenticationServiceCollectionExtensions
{
    internal static IServiceCollection AddAdminAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var configurationSection = configuration.GetSection(AdminOpenIdConnectOptions.SectionName);
        var settings = configurationSection
            .Get<AdminOpenIdConnectOptions>() ?? new AdminOpenIdConnectOptions();

        services.AddSingleton<
            IValidateOptions<AdminOpenIdConnectOptions>,
            AdminOpenIdConnectOptionsValidator>();
        services
            .AddOptions<AdminOpenIdConnectOptions>()
            .Bind(configurationSection)
            .ValidateOnStart();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthenticationConstants.SessionScheme;
                options.DefaultSignInScheme = AuthenticationConstants.SessionScheme;
                options.DefaultChallengeScheme = AuthenticationConstants.OpenIdConnectScheme;
            })
            .AddCookie(AuthenticationConstants.SessionScheme, options =>
            {
                options.Cookie.Name = ".TacticalHeroes.Admin.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = IdentityRoutes.Login;
                options.AccessDeniedPath = IdentityRoutes.Login;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            })
            .AddOpenIdConnect(AuthenticationConstants.OpenIdConnectScheme, options =>
            {
                options.SignInScheme = AuthenticationConstants.SessionScheme;
                options.Authority = settings.Authority.TrimEnd('/');
                options.ClientId = settings.ClientId;
                options.CallbackPath = settings.CallbackPath;
                options.SignedOutCallbackPath = settings.SignedOutCallbackPath;
                options.SignedOutRedirectUri = AdminRoutes.Home;
                options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Require;
                options.MapInboundClaims = false;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "role";
                options.Scope.Clear();
                options.Scope.Add(OpenIdConnectScope.OpenId);
                options.Scope.Add(OpenIdConnectScope.OfflineAccess);
                options.Scope.Add(OpenIdConnectScope.Profile);
                options.Scope.Add(OpenIdConnectScope.Email);
                options.Scope.Add("roles");
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        context.ProtocolMessage.IssuerAddress = BuildBffEndpoint(
                            context.Request,
                            "/connect/authorize");
                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        context.ProtocolMessage.IssuerAddress = BuildBffEndpoint(
                            context.Request,
                            "/connect/logout");
                        return Task.CompletedTask;
                    },
                    OnRemoteFailure = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect(IdentityRoutes.LoginPage(error: LoginError.OAuth));
                        return Task.CompletedTask;
                    },
                };
            });

        services.ConfigureCookieOidc(
            AuthenticationConstants.SessionScheme,
            AuthenticationConstants.OpenIdConnectScheme);
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthenticationConstants.ApiAuthorizationPolicy,
                policy => policy.RequireAuthenticatedUser());
        });
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

    private static string BuildBffEndpoint(HttpRequest request, string path)
    {
        return UriHelper.BuildAbsolute(
            request.Scheme,
            request.Host,
            request.PathBase,
            path);
    }
}
