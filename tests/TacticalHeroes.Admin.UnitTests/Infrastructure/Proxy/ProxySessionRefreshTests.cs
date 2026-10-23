using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using TacticalHeroes.Admin.Infrastructure.Authentication;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Session;
using TacticalHeroes.Admin.Infrastructure.Proxy;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Proxy;

public sealed class ProxySessionRefreshTests
{
    [Theory(DisplayName = "Proxy should refresh only current session tokens when password response is received")]
    [InlineData("POST", "/api/v1/auth/change-password", 204, true, true, false, true)]
    [InlineData("POST", "/api/v1/auth/change-password", 400, true, true, false, false)]
    [InlineData("POST", "/api/v1/users", 204, true, true, false, false)]
    [InlineData("GET", "/api/v1/auth/change-password", 204, true, true, false, false)]
    [InlineData("POST", "/api/v1/auth/change-password", 204, false, true, false, false)]
    [InlineData("POST", "/api/v1/auth/change-password", 204, true, false, false, false)]
    [InlineData("POST", "/api/v1/auth/change-password", 204, true, true, true, true)]
    public async Task Proxy_Should_RefreshOnlyCurrentSessionTokens_When_PasswordResponseIsReceived(
        string method, string path, int status, bool attachToken, bool signedIn, bool refreshFails, bool refreshExpected)
    {
        var ct = TestContext.Current.CancellationToken;
        var backendBuilder = WebApplication.CreateSlimBuilder();
        backendBuilder.Logging.ClearProviders();
        await using var backend = backendBuilder.Build();
        backend.Urls.Add("http://127.0.0.1:0");
        backend.MapGet("/api/v1/probe", (HttpContext context) => Results.Text(
            context.Request.Headers.Authorization.ToString()));
        backend.MapMethods("/api/v1/{**remainder}", ["GET", "POST"], () => Results.StatusCode(status));
        await backend.StartAsync(ct);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("proxy-session-refresh-test-key-32-bytes"));
        using var tokenHandler = new TokenEndpointHandler(key, refreshFails);
        using var backchannel = new HttpClient(tokenHandler);
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
        var proxyBuilder = WebApplication.CreateSlimBuilder();
        proxyBuilder.Logging.ClearProviders();
        proxyBuilder.Configuration.Sources.Clear();
        proxyBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ReverseProxy:Routes:api:ClusterId"] = "backend",
            ["ReverseProxy:Routes:api:Match:Path"] = "/api/v1/{**remainder}",
            ["ReverseProxy:Routes:api:Metadata:AttachSessionAccessToken"] = attachToken.ToString(),
            ["ReverseProxy:Clusters:backend:Destinations:primary:Address"] = backend.Urls.Single()
        });
        proxyBuilder.Services.AddAuthentication(AuthenticationConstants.SessionScheme)
            .AddCookie(AuthenticationConstants.SessionScheme);
        proxyBuilder.Services.AddAuthorization();
        proxyBuilder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        proxyBuilder.Services.AddSingleton(new CookieOidcRefresher(
            new FixedOptionsMonitor(oidcOptions),
            Microsoft.Extensions.Options.Options.Create(new AdminOpenIdConnectOptions())));
        proxyBuilder.Services.AddSingleton<SessionCookieRefresher>();
        proxyBuilder.Services.AddTacticalHeroesProxy(proxyBuilder.Configuration);
        await using var proxy = proxyBuilder.Build();
        proxy.Urls.Add("http://127.0.0.1:0");
        proxy.UseAuthentication();
        proxy.UseAuthorization();
        proxy.MapReverseProxy();
        proxy.MapGet("/test-login", async (HttpContext context) =>
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
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        });
        proxy.MapGet("/session", async (HttpContext context) =>
            await context.Response.WriteAsJsonAsync(new SessionSnapshot(
                await context.GetTokenAsync(AuthenticationConstants.SessionScheme, "access_token"),
                context.User.FindFirst("permission")?.Value)));
        await proxy.StartAsync(ct);
        using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri(proxy.Urls.Single())
        };
        if (signedIn)
        {
            using var login = await client.GetAsync("/test-login", ct);
            login.EnsureSuccessStatusCode();
        }
        using var request = new HttpRequestMessage(new HttpMethod(method), path);

        using var response = await client.SendAsync(request, ct);

        ((int)response.StatusCode).ShouldBe(status);
        tokenHandler.Calls.ShouldBe(refreshExpected ? 1 : 0);
        var refreshed = refreshExpected && !refreshFails;
        var loggedIn = signedIn && !(refreshExpected && refreshFails);
        var session = await client.GetFromJsonAsync<SessionSnapshot>("/session", ct);
        session.ShouldNotBeNull();
        if (loggedIn)
        {
            session.AccessToken.ShouldBe(refreshed ? "new-access" : "old-access");
            session.Permission.ShouldBe(refreshed ? "new.permission" : "old.permission");
        }
        else
        {
            session.AccessToken.ShouldBeNull();
            session.Permission.ShouldBeNull();
        }
        var authorization = await client.GetStringAsync("/api/v1/probe", ct);
        authorization.ShouldBe(loggedIn && attachToken ? $"Bearer {session.AccessToken}" : "");
        if (refreshExpected)
        {
            tokenHandler.RefreshToken.ShouldBe("old-refresh");
            response.Headers.Contains("Set-Cookie").ShouldBeTrue();
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

        internal string? RefreshToken { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            var content = await request.Content!.ReadAsStringAsync(cancellationToken);
            RefreshToken = content.Split('&').Single(part => part.StartsWith("refresh_token=", StringComparison.Ordinal))
                .Split('=')[1];
            var idToken = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = "https://identity.example.test",
                Audience = "admin",
                Subject = new ClaimsIdentity([new Claim("sub", "user"), new Claim("permission", "new.permission")]),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            });
            return new HttpResponseMessage(fails ? HttpStatusCode.BadRequest : HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    access_token = "new-access",
                    refresh_token = "new-refresh",
                    id_token = idToken,
                    token_type = "Bearer",
                    expires_in = 900
                }), Encoding.UTF8, "application/json")
            };
        }
    }
}
