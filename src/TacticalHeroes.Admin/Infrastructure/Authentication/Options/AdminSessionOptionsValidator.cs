using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Options;

internal sealed class AdminSessionOptionsValidator
    : IValidateOptions<AdminSessionOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        AdminSessionOptions options)
    {
        List<string> failures = [];

        if (string.IsNullOrWhiteSpace(options.CookieName) ||
            !CookieHeaderValue.TryParse($"{options.CookieName}=value", out _))
        {
            failures.Add(
                $"{AdminSessionOptions.SectionName}:CookieName must be a valid cookie name.");
        }

        if (options.Lifetime <= TimeSpan.Zero)
        {
            failures.Add($"{AdminSessionOptions.SectionName}:Lifetime must be positive.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
