using System.Text.Json;
using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class ApiAdapterConventionTests
{
    private static readonly Regex MappedResponseVariableRegex = new(
        @"\.Map\(\s*(?<variable>[a-z][A-Za-z0-9_]*)\s*=>",
        RegexOptions.CultureInvariant);

    [Fact(DisplayName = "API adapter names match their OpenAPI tags")]
    public void ApiAdapters_Should_UseOpenApiTagName_When_Named()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(repositoryRoot, "openapi", "tactical-heroes.json")));
        HashSet<string> tags = document.RootElement.GetProperty("paths")
            .EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject())
            .Where(operation => operation.Value.TryGetProperty("tags", out _))
            .SelectMany(operation => operation.Value.GetProperty("tags").EnumerateArray())
            .Select(tag => tag.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        List<string> violations = [];

        foreach (string apiPath in Directory.EnumerateFiles(
                     modulesRoot,
                     "*Api.cs",
                     SearchOption.AllDirectories))
        {
            string apiName = Path.GetFileNameWithoutExtension(apiPath);
            string tag = apiName[..^"Api".Length];
            string source = File.ReadAllText(apiPath);

            if (!tags.Contains(tag))
            {
                violations.Add(
                    $"{Path.GetRelativePath(repositoryRoot, apiPath)}: " +
                    $"'{tag}' is not an OpenAPI tag");
            }

            if (!Regex.IsMatch(
                    source,
                    $@"\bpublic\s+sealed\s+class\s+{Regex.Escape(apiName)}\b",
                    RegexOptions.CultureInvariant))
            {
                violations.Add(
                    $"{Path.GetRelativePath(repositoryRoot, apiPath)}: " +
                    $"does not declare '{apiName}'");
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "API adapters delegate generated response mapping")]
    public void ApiAdapters_Should_DelegateGeneratedResponseMapping()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        List<string> violations = [];

        foreach (string apiPath in Directory.EnumerateFiles(
                     modulesRoot,
                     "*Api.cs",
                     SearchOption.AllDirectories))
        {
            string source = File.ReadAllText(apiPath);

            foreach (Match match in MappedResponseVariableRegex.Matches(source))
            {
                string variable = Regex.Escape(match.Groups["variable"].Value);
                if (Regex.IsMatch(
                        source,
                        $@"\b{variable}\s*!?\s*\.",
                        RegexOptions.CultureInvariant))
                {
                    violations.Add(
                        $"{Path.GetRelativePath(repositoryRoot, apiPath)}: " +
                        $"maps generated response members directly");
                }
            }
        }

        violations.ShouldBeEmpty();
    }
}
