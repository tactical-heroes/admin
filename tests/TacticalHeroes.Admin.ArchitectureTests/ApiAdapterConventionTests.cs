using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class ApiAdapterConventionTests
{
    [GeneratedRegex(
        @"\.Map\(\s*(?<variable>[a-z][A-Za-z0-9_]*)\s*=>",
        RegexOptions.CultureInvariant)]
    private static partial Regex MappedResponseVariableRegex();

    [Fact(DisplayName = "API adapter types match their filenames")]
    public void ApiAdapters_Should_DeclareTypeMatchingFilename()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        List<string> violations = [];

        foreach (string apiPath in Directory.EnumerateFiles(
                     modulesRoot,
                     "*Api.cs",
                     SearchOption.AllDirectories))
        {
            string apiName = Path.GetFileNameWithoutExtension(apiPath);
            string source = File.ReadAllText(apiPath);

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

            foreach (Match match in MappedResponseVariableRegex().Matches(source))
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
