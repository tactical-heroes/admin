using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using TacticalHeroes.Admin.Infrastructure.Authentication;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options.Session;
using TacticalHeroes.Admin.Infrastructure.Authentication.Session;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.OpenIdConnect;

public sealed class CookieOidcRefresherTests
{
    private const string SigningKeyText = "test-signing-key-for-session-refresh-32-bytes";

    [Theory(DisplayName = "Cookie refresh should respect refresh trigger when session is validated")]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public async Task ValidateOrRefreshCookieAsync_Should_RespectRefreshTrigger_When_SessionIsValidated(
        bool force, bool expired)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKeyText));
        var handler = new TokenEndpointHandler(signingKey, HttpStatusCode.OK);
        using var httpClient = new HttpClient(handler);
        var (refresher, context) = CreateRefresh(httpClient, signingKey);
        if (expired)
        {
            context.Properties.UpdateTokenValue(
                "expires_at", DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O", CultureInfo.InvariantCulture));
        }
        var shouldRefresh = force || expired;

        await refresher.ValidateOrRefreshCookieAsync(context, "oidc", forceRefresh: force);

        handler.Calls.ShouldBe(shouldRefresh ? 1 : 0);
        context.ShouldRenew.ShouldBe(shouldRefresh);
        context.Principal!.FindFirst("permission")!.Value.ShouldBe(shouldRefresh ? "new.permission" : "old.permission");
        context.Properties.GetTokenValue("access_token").ShouldBe(shouldRefresh ? "new-access" : "old-access");
        context.Properties.GetTokenValue("refresh_token").ShouldBe(shouldRefresh ? "new-refresh" : "old-refresh");
    }

    [Theory(DisplayName = "Cookie refresh should respect request path when session cookie is validated")]
    [InlineData("/authentication/refresh", true)]
    [InlineData("/", false)]
    public async Task ValidateOrRefreshCookieAsync_Should_RespectRequestPath_When_SessionCookieIsValidated(
        string path, bool shouldRefresh)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKeyText));
        var handler = new TokenEndpointHandler(signingKey, HttpStatusCode.OK);
        using var httpClient = new HttpClient(handler);
        var (refresher, context) = CreateRefresh(httpClient, signingKey);
        var sessionOptions = new AdminSessionOptions
        {
            CookieName = "test.session",
            Lifetime = TimeSpan.FromHours(1)
        };
        var setup = new SessionCookieOptionsSetup(
            Microsoft.Extensions.Options.Options.Create(sessionOptions), refresher);
        setup.Configure(AuthenticationConstants.SessionScheme, context.Options);
        context.HttpContext.Request.Path = path;

        await context.Options.Events.OnValidatePrincipal(context);

        handler.Calls.ShouldBe(shouldRefresh ? 1 : 0);
        context.ShouldRenew.ShouldBe(shouldRefresh);
    }

    [Fact(DisplayName = "Cookie refresh should reject principal when refresh fails")]
    public async Task ValidateOrRefreshCookieAsync_Should_RejectPrincipal_When_RefreshFails()
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKeyText));
        using var httpClient = new HttpClient(new TokenEndpointHandler(signingKey, HttpStatusCode.BadRequest));
        var (refresher, context) = CreateRefresh(httpClient, signingKey);

        await refresher.ValidateOrRefreshCookieAsync(context, "oidc", forceRefresh: true);

        context.Principal.ShouldBeNull();
        context.ShouldRenew.ShouldBeFalse();
    }

    [Fact(DisplayName = "Cookie refresh should report configuration error when configuration manager is missing")]
    public async Task ValidateOrRefreshCookieAsync_Should_ReportConfigurationError_When_ConfigurationManagerIsMissing()
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKeyText));
        var handler = new TokenEndpointHandler(signingKey, HttpStatusCode.OK);
        using var httpClient = new HttpClient(handler);
        var (refresher, context) = CreateRefresh(httpClient, signingKey, configureManager: false);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            refresher.ValidateOrRefreshCookieAsync(context, "oidc", forceRefresh: true));

        exception.Message.ShouldBe("The OIDC configuration manager is missing.");
        handler.Calls.ShouldBe(0);
    }

    [Theory(DisplayName = "Cookie refresh should preserve principal when token endpoint is temporarily unavailable")]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task ValidateOrRefreshCookieAsync_Should_PreservePrincipal_When_TokenEndpointIsTemporarilyUnavailable(
        HttpStatusCode status)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKeyText));
        using var httpClient = new HttpClient(new TokenEndpointHandler(signingKey, status));
        var (refresher, context) = CreateRefresh(httpClient, signingKey);

        var error = await Should.ThrowAsync<HttpRequestException>(() =>
            refresher.ValidateOrRefreshCookieAsync(context, "oidc", forceRefresh: true));

        error.StatusCode.ShouldBe(status);
        context.Principal.ShouldNotBeNull();
        context.Principal.FindFirst("permission")!.Value.ShouldBe("old.permission");
        context.ShouldRenew.ShouldBeFalse();
        context.Properties.GetTokenValue("access_token").ShouldBe("old-access");
        context.Properties.GetTokenValue("refresh_token").ShouldBe("old-refresh");
    }

    private static (CookieOidcRefresher Refresher, CookieValidatePrincipalContext Context) CreateRefresh(
        HttpClient httpClient, SecurityKey signingKey, bool configureManager = true)
    {
        var options = new OpenIdConnectOptions
        {
            ClientId = "admin",
            Backchannel = httpClient,
            TimeProvider = TimeProvider.System,
            ConfigurationManager = configureManager
                ? new StaticConfigurationManager<OpenIdConnectConfiguration>(
                    new OpenIdConnectConfiguration
                    {
                        Issuer = "https://identity.example.test",
                        TokenEndpoint = "https://identity.example.test/connect/token"
                    })
                : null,
            TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = "admin",
                IssuerSigningKey = signingKey
            }
        };
        var monitor = new FixedOptionsMonitor(options);
        var refresher = new CookieOidcRefresher(
            monitor, Microsoft.Extensions.Options.Options.Create(new AdminOpenIdConnectOptions()));
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
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "user"), new Claim("permission", "old.permission")], "session"));
        return (refresher, new CookieValidatePrincipalContext(
            new DefaultHttpContext(),
            new AuthenticationScheme("session", null, typeof(CookieAuthenticationHandler)),
            new CookieAuthenticationOptions(),
            new AuthenticationTicket(principal, properties, "session")));
    }

    private sealed class FixedOptionsMonitor(OpenIdConnectOptions options) : IOptionsMonitor<OpenIdConnectOptions>
    {
        public OpenIdConnectOptions CurrentValue => options;

        public OpenIdConnectOptions Get(string? name) => options;
        public IDisposable? OnChange(Action<OpenIdConnectOptions, string?> listener) => null;
    }

    private sealed class TokenEndpointHandler(SecurityKey key, HttpStatusCode status) : HttpMessageHandler
    {
        internal int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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
            return Task.FromResult(new HttpResponseMessage(status)
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
