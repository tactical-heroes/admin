using Microsoft.Extensions.Options;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Options;

internal sealed class AdminOpenIdConnectOptionsValidator
    : IValidateOptions<AdminOpenIdConnectOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        AdminOpenIdConnectOptions options)
    {
        List<string> failures = [];

        if (!Uri.TryCreate(options.Authority, UriKind.Absolute, out var authority) ||
            !IsHttpScheme(authority))
        {
            failures.Add(
                $"{AdminOpenIdConnectOptions.SectionName}:Authority must be an absolute HTTP or HTTPS URI.");
        }

        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            failures.Add($"{AdminOpenIdConnectOptions.SectionName}:ClientId is required.");
        }

        ValidatePath(
            options.CallbackPath,
            nameof(AdminOpenIdConnectOptions.CallbackPath),
            failures);
        ValidatePath(
            options.SignedOutCallbackPath,
            nameof(AdminOpenIdConnectOptions.SignedOutCallbackPath),
            failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }

    private static void ValidatePath(
        string value,
        string name,
        ICollection<string> failures)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !value.StartsWith('/') ||
            value.StartsWith("//", StringComparison.Ordinal))
        {
            failures.Add(
                $"{AdminOpenIdConnectOptions.SectionName}:{name} must be an absolute application path.");
        }
    }

    private static bool IsHttpScheme(Uri uri)
    {
        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
