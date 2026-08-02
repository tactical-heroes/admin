using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class HttpClientConfigurationConventionTests
{
    private static readonly string[] RuntimeConfigurationPaths =
    [
        "src/TacticalHeroes.Admin/appsettings.json",
        "src/TacticalHeroes.Admin.Client/wwwroot/appsettings.json",
    ];

    private static readonly Regex HttpClientTimeoutAssignmentRegex = new(
        @"\b\w*client\s*\.\s*Timeout\s*=\s*(?<value>[^;]+);",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    [Fact(DisplayName = "HTTP client timeouts use the configured request timeout")]
    public void HttpClientTimeouts_Should_UseRequestTimeout_When_SourceIsScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        List<string> assignments = [];
        List<string> violations = [];

        foreach (string sourcePath in EnumerateSourceFiles(sourceRoot))
        {
            string source = File.ReadAllText(sourcePath);

            foreach (Match match in HttpClientTimeoutAssignmentRegex.Matches(source))
            {
                string relativePath = Path.GetRelativePath(repositoryRoot, sourcePath);
                string assignedValue = match.Groups["value"].Value.Trim();
                assignments.Add(relativePath);

                if (!assignedValue.Equals("requestTimeout", StringComparison.Ordinal))
                {
                    violations.Add($"{relativePath}: {assignedValue}");
                }
            }
        }

        assignments.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Server and WebAssembly configurations define the API timeout")]
    public void RuntimeConfigurations_Should_DefineApiTimeout_When_ConfigurationsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        List<string> violations = [];

        foreach (string relativePath in RuntimeConfigurationPaths)
        {
            string configurationPath = Path.Combine(
                repositoryRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar));

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(configurationPath));

            if (!document.RootElement.TryGetProperty("TacticalHeroesApi", out JsonElement api) ||
                !api.TryGetProperty("Timeout", out JsonElement timeoutElement) ||
                timeoutElement.ValueKind is not JsonValueKind.String ||
                !TimeSpan.TryParse(
                    timeoutElement.GetString(),
                    CultureInfo.InvariantCulture,
                    out TimeSpan timeout) ||
                timeout <= TimeSpan.Zero)
            {
                violations.Add(relativePath);
            }
        }

        violations.ShouldBeEmpty();
    }

    private static IEnumerable<string> EnumerateSourceFiles(string root)
    {
        return Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(static path =>
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase) &&
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase));
    }
}
