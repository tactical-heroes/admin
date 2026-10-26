using System.Collections.Concurrent;
using System.Diagnostics;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Trace;

using PANiXiDA.Core.Observability;

using TacticalHeroes.Admin.Infrastructure.Proxy;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Proxy;

public sealed class ProxyTraceContextTests
{
    [Theory(DisplayName = "Proxy should correlate exported spans and preserve upstream sampling when host tracing is configured")]
    [InlineData(null, null)]
    [InlineData("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", "vendor=sampled")]
    [InlineData("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00", "vendor=unsampled")]
    public async Task Proxy_Should_CorrelateSpansAndPreserveSampling_When_HostTracingIsConfigured(
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
            ["OTEL_EXPORTER_OTLP_TIMEOUT"] = "100",
            ["ReverseProxy:Routes:probe:ClusterId"] = "backend",
            ["ReverseProxy:Routes:probe:Match:Path"] = "/probe",
            ["ReverseProxy:Clusters:backend:Destinations:primary:Address"] = backend.Urls.Single()
        });
        var activities = new ConcurrentBag<Activity>();
        using var exporter = new ActivityExporter(activities);
        proxyBuilder.AddObservability();
        proxyBuilder.Services.AddOpenTelemetry().WithTracing(tracing =>
            tracing.AddProcessor(new SimpleActivityExportProcessor(exporter)));
        proxyBuilder.Services.AddTacticalHeroesProxy(proxyBuilder.Configuration);
        await using var proxy = proxyBuilder.Build();
        proxy.Urls.Add("http://127.0.0.1:0");
        ActivityContext proxyContext = default;
        proxy.Use(async (_, next) =>
        {
            proxyContext = Activity.Current!.Context;
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
        await proxy.StopAsync(cancellationToken);

        response.EnsureSuccessStatusCode();
        var headers = receivedContext.Split('|');
        ActivityContext.TryParse(headers[0], headers[1], out var downstreamContext).ShouldBeTrue();
        downstreamContext.TraceId.ShouldBe(proxyContext.TraceId);
        downstreamContext.SpanId.ShouldNotBe(proxyContext.SpanId);
        downstreamContext.TraceFlags.ShouldBe(proxyContext.TraceFlags);
        headers[1].ShouldBe(traceState ?? "");

        if (traceParent is not null)
        {
            ActivityContext.TryParse(traceParent, traceState, out var upstreamContext).ShouldBeTrue();
            proxyContext.TraceId.ShouldBe(upstreamContext.TraceId);
            downstreamContext.TraceFlags.ShouldBe(upstreamContext.TraceFlags);
        }

        if (downstreamContext.TraceFlags.HasFlag(ActivityTraceFlags.Recorded))
        {
            activities.ShouldContain(activity =>
                activity.Source.Name == "Yarp.ReverseProxy" && activity.TraceId == downstreamContext.TraceId);
            activities.ShouldContain(activity =>
                activity.Kind == ActivityKind.Client && activity.SpanId == downstreamContext.SpanId);
        }
        else
        {
            activities.ShouldNotContain(activity => activity.TraceId == downstreamContext.TraceId);
        }
    }

    private sealed class ActivityExporter(ConcurrentBag<Activity> activities) : BaseExporter<Activity>
    {
        public override ExportResult Export(in Batch<Activity> batch)
        {
            foreach (var activity in batch)
            {
                activities.Add(activity);
            }

            return ExportResult.Success;
        }
    }
}
