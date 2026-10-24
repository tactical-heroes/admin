using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TacticalHeroes.Admin.Infrastructure.Authentication.Endpoints;
using TacticalHeroes.Admin.Infrastructure.Authentication.Login;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Session;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.Endpoints;

public sealed class AuthenticationEndpointRouteBuilderExtensionsTests
{
    [Theory(DisplayName = "Refresh should redirect only locally when return url is provided")]
    [InlineData(null, true, "/")]
    [InlineData("", true, "/")]
    [InlineData("/users?page=2&email=admin%40example.test#list", true,
        "/users?page=2&email=admin%40example.test#list")]
    [InlineData("~/users", true, "/users")]
    [InlineData("https://external.example.test/users", true, "/")]
    [InlineData("//external.example.test/users", true, "/")]
    [InlineData("/\\external.example.test/users", true, "/")]
    [InlineData("users", true, "/")]
    [InlineData("/users", false, null)]
    public async Task Refresh_Should_RedirectOnlyLocally_When_ReturnUrlIsProvided(
        string? returnUrl, bool authenticated, string? expectedLocation)
    {
        var ct = TestContext.Current.CancellationToken;
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthentication("test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddHttpClient<IdentityLoginGateway>();
        builder.Services.AddSingleton<OpenIdConnectEndpointResolver>();
        builder.Services.AddSingleton<SessionValidator>();
        await using var app = builder.Build();
        app.Urls.Add("http://127.0.0.1:0");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapAdminAuthentication();
        await app.StartAsync(ct);
        using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri(app.Urls.Single())
        };
        if (authenticated)
        {
            client.DefaultRequestHeaders.Add("X-Test-User", "user");
        }
        string path = returnUrl is null
            ? "/authentication/refresh"
            : $"/authentication/refresh?returnUrl={Uri.EscapeDataString(returnUrl)}";

        using var response = await client.GetAsync(path, ct);

        response.StatusCode.ShouldBe(authenticated ? HttpStatusCode.Redirect : HttpStatusCode.Unauthorized);
        response.Headers.Location?.OriginalString.ShouldBe(expectedLocation);
    }

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var result = Request.Headers.ContainsKey("X-Test-User")
                ? AuthenticateResult.Success(new AuthenticationTicket(
                    new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user")], Scheme.Name)), Scheme.Name))
                : AuthenticateResult.NoResult();
            return Task.FromResult(result);
        }
    }
}
