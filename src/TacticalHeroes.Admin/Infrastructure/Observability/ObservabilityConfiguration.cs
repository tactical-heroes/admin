using PANiXiDA.Core.Observability;

namespace TacticalHeroes.Admin.Infrastructure.Observability;

internal static class ObservabilityConfiguration
{
    internal static WebApplicationBuilder AddAdminObservability(this WebApplicationBuilder builder)
    {
        builder.AddObservability();
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics.AddMeter(
                "Microsoft.AspNetCore.Components",
                "Microsoft.AspNetCore.Components.Lifecycle",
                "Microsoft.AspNetCore.Components.Server.Circuits"))
            .WithTracing(tracing => tracing.AddSource("Yarp.ReverseProxy"));

        return builder;
    }
}
