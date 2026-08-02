using Microsoft.Extensions.Options;

namespace TacticalHeroes.Admin.Client.App.Options;

internal sealed class TacticalHeroesApiClientOptionsValidator
    : IValidateOptions<TacticalHeroesApiClientOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        TacticalHeroesApiClientOptions options)
    {
        List<string> failures = [];

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri) ||
            !IsHttpScheme(baseUri))
        {
            failures.Add(
                $"{TacticalHeroesApiClientOptions.SectionName}:BaseUrl must be an absolute HTTP or HTTPS URI.");
        }

        if (options.Timeout <= TimeSpan.Zero ||
            options.Timeout.TotalMilliseconds > int.MaxValue)
        {
            failures.Add(
                $"{TacticalHeroesApiClientOptions.SectionName}:Timeout must be a positive duration supported by HttpClient.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }

    private static bool IsHttpScheme(Uri uri)
    {
        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
