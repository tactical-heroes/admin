using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Metrics;

using TacticalHeroes.Admin.Infrastructure.Observability;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Observability;

public sealed class ObservabilityConfigurationTests
{
    [Fact(DisplayName = "Observability should export runtime and all Blazor meter measurements when metrics are collected")]
    public async Task Observability_Should_ExportRuntimeAndBlazorMeasurements_When_MetricsAreCollected()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_TIMEOUT"] = "100"
        });
        builder.AddAdminObservability();
        var metrics = new ConcurrentBag<(string Meter, string Name)>();
        using var exporter = new MetricExporter(metrics);
        using var reader = new BaseExportingMetricReader(exporter);
        builder.Services.AddOpenTelemetry().WithMetrics(provider => provider.AddReader(reader));
        await using var app = builder.Build();
        _ = app.Services.GetRequiredService<MeterProvider>();
        using var components = new Meter("Microsoft.AspNetCore.Components");
        using var lifecycle = new Meter("Microsoft.AspNetCore.Components.Lifecycle");
        using var circuits = new Meter("Microsoft.AspNetCore.Components.Server.Circuits");

        components.CreateCounter<long>("admin.test.navigation").Add(1);
        lifecycle.CreateCounter<long>("admin.test.render").Add(1);
        circuits.CreateUpDownCounter<long>("admin.test.circuit").Add(1);
        reader.Collect().ShouldBeTrue();

        metrics.ShouldContain(("System.Runtime", "dotnet.process.memory.working_set"));
        metrics.ShouldContain((components.Name, "admin.test.navigation"));
        metrics.ShouldContain((lifecycle.Name, "admin.test.render"));
        metrics.ShouldContain((circuits.Name, "admin.test.circuit"));
    }

    private sealed class MetricExporter(ConcurrentBag<(string Meter, string Name)> metrics) : BaseExporter<Metric>
    {
        public override ExportResult Export(in Batch<Metric> batch)
        {
            foreach (var metric in batch)
            {
                metrics.Add((metric.MeterName, metric.Name));
            }

            return ExportResult.Success;
        }
    }
}
