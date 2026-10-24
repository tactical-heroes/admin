using System.Net;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using TacticalHeroes.Admin.Infrastructure.Authentication;
using TacticalHeroes.Admin.Infrastructure.Authentication.Endpoints;
using TacticalHeroes.Admin.Infrastructure.Authentication.Login;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Session;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.Session;

public sealed class SessionValidatorTests
{
    [Theory(DisplayName = "Session endpoint should validate current token and clear cookie only when session is invalid")]
    [InlineData(true, true, 200, 204, true)]
    [InlineData(true, true, 401, 401, false)]
    [InlineData(true, true, 403, 401, false)]
    [InlineData(true, true, 500, 503, true)]
    [InlineData(true, true, 429, 503, true)]
    [InlineData(true, true, 0, 503, true)]
    [InlineData(true, true, -1, 503, true)]
    [InlineData(true, false, 200, 401, false)]
    [InlineData(false, false, 200, 401, false)]
    public async Task ValidateAsync_Should_ClearCookieOnly_When_SessionIsInvalid(
        bool authenticated, bool hasToken, int upstreamStatus, int expectedStatus, bool remainsAuthenticated)
    {
        var ct = TestContext.Current.CancellationToken;
        using var handler = new UserInfoHandler(upstreamStatus);
        using var backchannel = new HttpClient(handler);
        var options = new OpenIdConnectOptions
        {
            Backchannel = backchannel,
            ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(
                new OpenIdConnectConfiguration
                {
                    UserInfoEndpoint = "https://identity.example.test/connect/userinfo"
                })
        };
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthentication(AuthenticationConstants.SessionScheme)
            .AddCookie(AuthenticationConstants.SessionScheme, cookie => cookie.Cookie.SecurePolicy = CookieSecurePolicy.None);
        builder.Services.AddAuthorization();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        builder.Services.AddSingleton<IOptionsMonitor<OpenIdConnectOptions>>(new FixedOptionsMonitor(options));
        builder.Services.AddSingleton<SessionValidator>();
        builder.Services.AddSingleton<OpenIdConnectEndpointResolver>();
        builder.Services.AddHttpClient<IdentityLoginGateway>();
        await using var app = builder.Build();
        app.Urls.Add("http://127.0.0.1:0");
        app.UseStatusCodePagesWithReExecute("/error");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapAdminAuthentication();
        app.MapGet("/error", () => Results.StatusCode(418));
        app.MapGet("/probe", (HttpContext context) => context.User.Identity?.IsAuthenticated == true);
        app.MapGet("/login-test", async (HttpContext context) =>
        {
            var properties = new AuthenticationProperties();
            if (hasToken)
            {
                properties.StoreTokens([new AuthenticationToken { Name = "access_token", Value = "session-access" }]);
            }

            await context.SignInAsync(AuthenticationConstants.SessionScheme,
                new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user")], "session")), properties);
        });
        await app.StartAsync(ct);
        using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri(app.Urls.Single())
        };
        if (authenticated)
        {
            using var login = await client.GetAsync("/login-test", ct);
            login.EnsureSuccessStatusCode();
        }

        using var response = await client.GetAsync("/authentication/session", ct);

        ((int)response.StatusCode).ShouldBe(expectedStatus);
        response.Headers.CacheControl!.NoStore.ShouldBeTrue();
        response.Headers.Location.ShouldBeNull();
        response.Headers.Contains(SessionPermissionsVersion.HeaderName).ShouldBe(expectedStatus == 204);
        bool.Parse(await client.GetStringAsync("/probe", ct)).ShouldBe(remainsAuthenticated);
        handler.Calls.ShouldBe(authenticated && hasToken ? 1 : 0);
        if (handler.Calls > 0)
        {
            handler.Authorization.ShouldBe("Bearer session-access");
            handler.RequestUri.ShouldBe(new Uri("https://identity.example.test/connect/userinfo"));
        }
    }

    private sealed class FixedOptionsMonitor(OpenIdConnectOptions options) : IOptionsMonitor<OpenIdConnectOptions>
    {
        public OpenIdConnectOptions CurrentValue => options;

        public OpenIdConnectOptions Get(string? name) => options;
        public IDisposable? OnChange(Action<OpenIdConnectOptions, string?> listener) => null;
    }

    private sealed class UserInfoHandler(int status) : HttpMessageHandler
    {
        internal int Calls { get; private set; }
        internal string? Authorization { get; private set; }
        internal Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Authorization = request.Headers.Authorization?.ToString();
            RequestUri = request.RequestUri;
            return status switch
            {
                0 => throw new HttpRequestException("Identity provider is unavailable."),
                -1 => throw new TaskCanceledException("Identity provider timed out."),
                _ => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status))
            };
        }
    }
}
