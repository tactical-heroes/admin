namespace TacticalHeroes.Admin.Infrastructure.Observability.Options.BrowserTelemetry;

internal sealed class BrowserTelemetryOptions
{
    public const string SectionName = "BrowserTelemetry";

    public int MaxRequestBodySize { get; set; } = 1024 * 1024;
}
