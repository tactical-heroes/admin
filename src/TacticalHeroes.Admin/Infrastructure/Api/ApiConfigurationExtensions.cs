namespace TacticalHeroes.Admin.Infrastructure.Api;

internal static class ApiConfigurationExtensions
{
    private const string BaseUrlKey = "TacticalHeroesApi:BaseUrl";

    public static Uri GetTacticalHeroesApiBaseUri(this IConfiguration configuration)
    {
        var value = configuration[BaseUrlKey];

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException(
                $"Configuration value '{BaseUrlKey}' must be an absolute HTTP or HTTPS URL.");
        }

        return uri;
    }
}
