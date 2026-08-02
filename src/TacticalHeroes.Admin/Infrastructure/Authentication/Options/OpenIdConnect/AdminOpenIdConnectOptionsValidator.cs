using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Options.OpenIdConnect;

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

        if (options.RefreshBeforeExpiration <= TimeSpan.Zero)
        {
            failures.Add(
                $"{AdminOpenIdConnectOptions.SectionName}:RefreshBeforeExpiration must be positive.");
        }

        ValidateRequiredValue(
            options.NameClaimType,
            nameof(AdminOpenIdConnectOptions.NameClaimType),
            failures);
        ValidateRequiredValue(
            options.RoleClaimType,
            nameof(AdminOpenIdConnectOptions.RoleClaimType),
            failures);
        ValidateScopes(options.Scopes, failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }

    private static void ValidateRequiredValue(
        string value,
        string name,
        List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add($"{AdminOpenIdConnectOptions.SectionName}:{name} is required.");
        }
    }

    private static void ValidateScopes(
        List<string> scopes,
        List<string> failures)
    {
        if (scopes.Count == 0 || scopes.Any(string.IsNullOrWhiteSpace))
        {
            failures.Add(
                $"{AdminOpenIdConnectOptions.SectionName}:Scopes must contain only non-empty values.");
            return;
        }

        if (scopes.Distinct(StringComparer.Ordinal).Count() != scopes.Count)
        {
            failures.Add(
                $"{AdminOpenIdConnectOptions.SectionName}:Scopes must not contain duplicates.");
        }

        ValidateRequiredScope(OpenIdConnectScope.OpenId, scopes, failures);
        ValidateRequiredScope(OpenIdConnectScope.OfflineAccess, scopes, failures);
    }

    private static void ValidateRequiredScope(
        string requiredScope,
        List<string> scopes,
        List<string> failures)
    {
        if (!scopes.Contains(requiredScope, StringComparer.Ordinal))
        {
            failures.Add(
                $"{AdminOpenIdConnectOptions.SectionName}:Scopes must contain '{requiredScope}'.");
        }
    }

    private static void ValidatePath(
        string value,
        string name,
        List<string> failures)
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
