using Microsoft.Extensions.Options;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Options.IdentityLogin;

internal sealed class IdentityLoginRouteOptionsValidator
    : IValidateOptions<IdentityLoginRouteOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        IdentityLoginRouteOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Path) ||
            !options.Path.StartsWith('/') ||
            options.Path.StartsWith("//", StringComparison.Ordinal))
        {
            return ValidateOptionsResult.Fail(
                $"{IdentityLoginRouteOptions.SectionName}:Path must be an absolute application path.");
        }

        return ValidateOptionsResult.Success;
    }
}
