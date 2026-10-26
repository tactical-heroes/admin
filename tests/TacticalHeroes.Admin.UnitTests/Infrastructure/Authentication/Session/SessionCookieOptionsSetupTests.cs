using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using PANiXiDA.Core.Observability;

using TacticalHeroes.Admin.Infrastructure.Authentication;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options.Session;
using TacticalHeroes.Admin.Infrastructure.Authentication.Session;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.Session;

public sealed class SessionCookieOptionsSetupTests
{
    private const string RefreshPath = "/authentication/refresh";

    [Fact(DisplayName = "ValidatePrincipal should not renew the cookie or refresh tokens when telemetry is exported")]
    public async Task ValidatePrincipal_Should_NotRenewCookieOrRefreshTokens_When_TelemetryIsExported()
    {
        var refresher = new CookieOidcRefresher(new FixedOptionsMonitor(new OpenIdConnectOptions()),
            Microsoft.Extensions.Options.Options.Create(new AdminOpenIdConnectOptions()));
        var setup = new SessionCookieOptionsSetup(
            Microsoft.Extensions.Options.Options.Create(new AdminSessionOptions { CookieName = "test-session", Lifetime = TimeSpan.FromHours(1) }), refresher);
        var options = new CookieAuthenticationOptions();
        setup.Configure(AuthenticationConstants.SessionScheme, options);
        var context = new DefaultHttpContext();
        context.Request.Path = "/telemetry/v1/logs";
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(BrowserTelemetryEndpointMetadata.Instance), "telemetry"));
        var scheme = new AuthenticationScheme(AuthenticationConstants.SessionScheme, null, typeof(CookieAuthenticationHandler));
        var properties = new AuthenticationProperties();
        properties.StoreTokens([new AuthenticationToken
        {
            Name = "expires_at",
            Value = DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O", CultureInfo.InvariantCulture)
        }]);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user")], "session")),
            properties, AuthenticationConstants.SessionScheme);
        var sliding = new CookieSlidingExpirationContext(context, scheme, options, ticket, TimeSpan.FromMinutes(40), TimeSpan.FromMinutes(20))
        {
            ShouldRenew = true
        };
        var validation = new CookieValidatePrincipalContext(context, scheme, options, ticket);

        await options.Events.OnCheckSlidingExpiration(sliding);
        await options.Events.OnValidatePrincipal(validation);

        sliding.ShouldRenew.ShouldBeFalse();
        validation.ShouldRenew.ShouldBeFalse();
        validation.Principal.ShouldNotBeNull();
    }

    [Theory(DisplayName = "ValidatePrincipal should renew or clear cookie when refresh endpoint is requested")]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task ValidatePrincipal_Should_RenewOrClearCookie_When_RefreshEndpointIsRequested(
        bool forceRefresh, bool refreshFails)
    {
        var ct = TestContext.Current.CancellationToken;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("session-refresh-test-key-32-bytes"));
        using var handler = new TokenEndpointHandler(key, refreshFails);
        using var backchannel = new HttpClient(handler);
        var oidcOptions = new OpenIdConnectOptions
        {
            ClientId = "admin",
            TimeProvider = TimeProvider.System,
            Backchannel = backchannel,
            ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(
                new OpenIdConnectConfiguration
                {
                    Issuer = "https://identity.example.test",
                    TokenEndpoint = "https://identity.example.test/connect/token"
                }),
            TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = "admin",
                IssuerSigningKey = key
            }
        };
        var refresher = new CookieOidcRefresher(
            new FixedOptionsMonitor(oidcOptions),
            Microsoft.Extensions.Options.Options.Create(new AdminOpenIdConnectOptions()));
        var setup = new SessionCookieOptionsSetup(
            Microsoft.Extensions.Options.Options.Create(new AdminSessionOptions
            {
                CookieName = "test-session",
                Lifetime = TimeSpan.FromHours(1)
            }), refresher);
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthentication(AuthenticationConstants.SessionScheme)
            .AddCookie(AuthenticationConstants.SessionScheme, options =>
            {
                setup.Configure(AuthenticationConstants.SessionScheme, options);
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            });
        builder.Services.AddAuthorization();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        await using var app = builder.Build();
        app.Urls.Add("http://127.0.0.1:0");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet(RefreshPath, () => Results.NoContent()).RequireAuthorization();
        app.MapGet("/login-test", async (HttpContext context) =>
        {
            var properties = new AuthenticationProperties();
            properties.StoreTokens(
            [
                new AuthenticationToken { Name = "access_token", Value = "old-access" },
                new AuthenticationToken { Name = "refresh_token", Value = "old-refresh" },
                new AuthenticationToken
                {
                    Name = "expires_at",
                    Value = DateTimeOffset.UtcNow.AddHours(1).ToString("O", CultureInfo.InvariantCulture)
                }
            ]);
            await context.SignInAsync(AuthenticationConstants.SessionScheme,
                new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim("sub", "user"), new Claim("permission", "old.permission")], "session")), properties);
        });
        app.MapGet("/probe", async (HttpContext context) =>
            await context.Response.WriteAsJsonAsync(new SessionSnapshot(
                await context.GetTokenAsync(AuthenticationConstants.SessionScheme, "access_token"),
                context.User.FindFirst("permission")?.Value)));
        await app.StartAsync(ct);
        using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri(app.Urls.Single())
        };
        using var login = await client.GetAsync("/login-test", ct);
        login.EnsureSuccessStatusCode();

        using var response = await client.GetAsync(forceRefresh ? RefreshPath : "/probe", ct);

        handler.Calls.ShouldBe(forceRefresh ? 1 : 0);
        var session = await client.GetFromJsonAsync<SessionSnapshot>("/probe", ct);
        session.ShouldNotBeNull();
        if (refreshFails)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            response.Headers.Contains("Set-Cookie").ShouldBeTrue();
            session.AccessToken.ShouldBeNull();
            session.Permission.ShouldBeNull();
            handler.Calls.ShouldBe(1);
        }
        else
        {
            response.IsSuccessStatusCode.ShouldBeTrue();
            response.Headers.Contains("Set-Cookie").ShouldBe(forceRefresh);
            session.AccessToken.ShouldBe(forceRefresh ? "new-access" : "old-access");
            session.Permission.ShouldBe(forceRefresh ? "new.permission" : "old.permission");
        }
    }

    private sealed record SessionSnapshot(string? AccessToken, string? Permission);

    private sealed class FixedOptionsMonitor(OpenIdConnectOptions options) : IOptionsMonitor<OpenIdConnectOptions>
    {
        public OpenIdConnectOptions CurrentValue => options;

        public OpenIdConnectOptions Get(string? name) => options;
        public IDisposable? OnChange(Action<OpenIdConnectOptions, string?> listener) => null;
    }

    private sealed class TokenEndpointHandler(SecurityKey key, bool fails) : HttpMessageHandler
    {
        internal int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            var idToken = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = "https://identity.example.test",
                Audience = "admin",
                Subject = new ClaimsIdentity([new Claim("sub", "user"), new Claim("permission", "new.permission")]),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            });
            return Task.FromResult(new HttpResponseMessage(fails ? HttpStatusCode.BadRequest : HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    access_token = "new-access",
                    refresh_token = "new-refresh",
                    id_token = idToken,
                    token_type = "Bearer",
                    expires_in = 900
                }), Encoding.UTF8, "application/json")
            });
        }
    }
}
