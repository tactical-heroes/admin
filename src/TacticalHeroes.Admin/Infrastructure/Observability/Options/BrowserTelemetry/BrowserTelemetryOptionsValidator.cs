using Microsoft.Extensions.Options;

namespace TacticalHeroes.Admin.Infrastructure.Observability.Options.BrowserTelemetry;

internal sealed class BrowserTelemetryOptionsValidator : IValidateOptions<BrowserTelemetryOptions>
{
    public ValidateOptionsResult Validate(string? name, BrowserTelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.MaxRequestBodySize > 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("BrowserTelemetry:MaxRequestBodySize must be positive.");
    }
}
