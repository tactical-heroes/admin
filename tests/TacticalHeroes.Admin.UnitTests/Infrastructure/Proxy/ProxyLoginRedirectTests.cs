using System.Net;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using TacticalHeroes.Admin.Infrastructure.Proxy;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Proxy;

public sealed class ProxyLoginRedirectTests
{
    [Theory(DisplayName = "Proxy should keep login local and preserve encoded parameters when provider redirects to login")]
    [InlineData("", "https")]
    [InlineData("/admin", "https")]
    [InlineData("", "http")]
    public async Task Proxy_Should_KeepLoginLocalAndPreserveEncodedParameters_When_ProviderRedirectsToLogin(
        string pathBase,
        string providerScheme)
    {
        const string query = "?returnUrl=https%3A%2F%2Flocalhost%3A5173%2Fconnect%2Fauthorize%3Fclient_id%3Dweb%26request_uri%3Durn%253Arequest%253Aa%252Fb&prompt=login";
        string location = $"{providerScheme}://dev.example.test/login{query}";

        var (actualStatusCode, actualLocation) = await SendProxyRequestAsync(
            location, HttpStatusCode.Redirect, "true", pathBase);

        actualStatusCode.ShouldBe(HttpStatusCode.Redirect);
        actualLocation.ShouldBe($"{pathBase}/login{query}");
    }

    [Theory(DisplayName = "Proxy should preserve response when redirect is unrelated to login page")]
    [InlineData("https://localhost:5173/oauth/callback?code=code&state=state", HttpStatusCode.Redirect)]
    [InlineData("https://dev.example.test/oauth/logout-callback", HttpStatusCode.Redirect)]
    [InlineData("https://external.example.test/other", HttpStatusCode.Redirect)]
    [InlineData("/login?returnUrl=%2Fconnect%2Fauthorize", HttpStatusCode.Redirect)]
    [InlineData("ftp://dev.example.test/login", HttpStatusCode.Redirect)]
    [InlineData("https://dev.example.test/login", HttpStatusCode.OK)]
    [InlineData("https://dev.example.test/login", HttpStatusCode.TemporaryRedirect)]
    [InlineData(null, HttpStatusCode.BadGateway)]
    public async Task Proxy_Should_PreserveResponse_When_RedirectIsUnrelatedToLoginPage(
        string? location,
        HttpStatusCode statusCode)
    {
        var (actualStatusCode, actualLocation) = await SendProxyRequestAsync(location, statusCode, "true", "");

        actualStatusCode.ShouldBe(statusCode);
        actualLocation.ShouldBe(location);
    }

    [Theory(DisplayName = "Proxy should preserve login redirect when route does not enable rewriting")]
    [InlineData(null)]
    [InlineData("false")]
    public async Task Proxy_Should_PreserveLoginRedirect_When_RouteDoesNotEnableRewriting(string? metadata)
    {
        const string location = "https://dev.example.test/login?returnUrl=%2Fconnect%2Fauthorize";

        var (actualStatusCode, actualLocation) = await SendProxyRequestAsync(
            location, HttpStatusCode.Redirect, metadata, "");

        actualStatusCode.ShouldBe(HttpStatusCode.Redirect);
        actualLocation.ShouldBe(location);
    }

    private static async Task<(HttpStatusCode StatusCode, string? Location)> SendProxyRequestAsync(
        string? location,
        HttpStatusCode statusCode,
        string? metadata,
        string pathBase)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var backendBuilder = WebApplication.CreateSlimBuilder();
        backendBuilder.Logging.ClearProviders();
        await using var backend = backendBuilder.Build();
        backend.Urls.Add("http://127.0.0.1:0");
        backend.MapGet("/connect/authorize", (HttpContext context) =>
        {
            context.Response.StatusCode = (int)statusCode;
            if (location is not null)
            {
                context.Response.Headers.Location = location;
            }

            return Task.CompletedTask;
        });
        await backend.StartAsync(cancellationToken);

        var proxyBuilder = WebApplication.CreateSlimBuilder();
        proxyBuilder.Logging.ClearProviders();
        proxyBuilder.Configuration.Sources.Clear();
        proxyBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ReverseProxy:Routes:oidc:ClusterId"] = "backend",
            ["ReverseProxy:Routes:oidc:Match:Path"] = "/connect/{**catch-all}",
            ["ReverseProxy:Routes:oidc:Metadata:RewriteLoginRedirect"] = metadata,
            ["ReverseProxy:Clusters:backend:Destinations:primary:Address"] = backend.Urls.Single()
        });
        proxyBuilder.Services.AddTacticalHeroesProxy(proxyBuilder.Configuration);
        await using var proxy = proxyBuilder.Build();
        proxy.Urls.Add("http://127.0.0.1:0");
        proxy.UsePathBase(pathBase);
        proxy.MapReverseProxy();
        await proxy.StartAsync(cancellationToken);
        using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false });

        using var response = await client.GetAsync(
            $"{proxy.Urls.Single()}{pathBase}/connect/authorize",
            cancellationToken);

        return (response.StatusCode, response.Headers.Location?.OriginalString);
    }
}
