using System.Diagnostics;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using TacticalHeroes.Admin.Infrastructure.Proxy;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Proxy;

public sealed class ProxyTraceContextTests
{
    [Theory(DisplayName = "Proxy should preserve trace context when host tracing is not configured")]
    [InlineData(null, null)]
    [InlineData("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "vendor=sampled")]
    [InlineData("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00", "vendor=unsampled")]
    public async Task Proxy_Should_PreserveTraceContext_When_HostTracingIsNotConfigured(
        string? traceParent, string? traceState)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var backendBuilder = WebApplication.CreateSlimBuilder();
        backendBuilder.Logging.ClearProviders();
        await using var backend = backendBuilder.Build();
        backend.Urls.Add("http://127.0.0.1:0");
        backend.MapGet("/probe", (HttpContext context) => Results.Text(
            $"{context.Request.Headers.TraceParent}|{context.Request.Headers.TraceState}"));
        await backend.StartAsync(cancellationToken);

        var proxyBuilder = WebApplication.CreateSlimBuilder();
        proxyBuilder.Logging.ClearProviders();
        proxyBuilder.Configuration.Sources.Clear();
        proxyBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ReverseProxy:Routes:probe:ClusterId"] = "backend",
            ["ReverseProxy:Routes:probe:Match:Path"] = "/probe",
            ["ReverseProxy:Clusters:backend:Destinations:primary:Address"] = backend.Urls.Single()
        });
        proxyBuilder.Services.AddTacticalHeroesProxy(proxyBuilder.Configuration);
        await using var proxy = proxyBuilder.Build();
        proxy.Urls.Add("http://127.0.0.1:0");
        proxy.Use(async (_, next) =>
        {
            using var activity = new Activity("Admin proxy request").Start();
            await next();
        });
        proxy.MapReverseProxy();
        await proxy.StartAsync(cancellationToken);
        using var client = new HttpClient(new SocketsHttpHandler { ActivityHeadersPropagator = null });
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{proxy.Urls.Single()}/probe");
        if (traceParent is not null)
        {
            request.Headers.Add("traceparent", traceParent);
            request.Headers.Add("tracestate", traceState);
        }

        using var response = await client.SendAsync(request, cancellationToken);
        string receivedContext = await response.Content.ReadAsStringAsync(cancellationToken);

        response.EnsureSuccessStatusCode();
        receivedContext.ShouldBe($"{traceParent}|{traceState}");
    }
}
