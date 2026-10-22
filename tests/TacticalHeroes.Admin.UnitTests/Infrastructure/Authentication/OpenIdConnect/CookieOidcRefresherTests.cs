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

using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options.OpenIdConnect;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.OpenIdConnect;

public sealed class CookieOidcRefresherTests
{
    private const string SigningKeyText = "test-signing-key-for-session-refresh-32-bytes";

    [Theory(DisplayName = "Cookie refresh should update tokens and permissions when refresh is forced")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidateOrRefreshCookieAsync_Should_UpdateSession_When_RefreshIsForced(bool force)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKeyText));
        var handler = new TokenEndpointHandler(signingKey, HttpStatusCode.OK);
        using var httpClient = new HttpClient(handler);
        var (refresher, context) = CreateRefresh(httpClient, signingKey);

        await refresher.ValidateOrRefreshCookieAsync(context, "oidc", forceRefresh: force);

        handler.Calls.ShouldBe(force ? 1 : 0);
        context.ShouldRenew.ShouldBe(force);
        context.Principal!.FindFirst("permission")!.Value.ShouldBe(force ? "new.permission" : "old.permission");
        context.Properties.GetTokenValue("access_token").ShouldBe(force ? "new-access" : "old-access");
        context.Properties.GetTokenValue("refresh_token").ShouldBe(force ? "new-refresh" : "old-refresh");
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
