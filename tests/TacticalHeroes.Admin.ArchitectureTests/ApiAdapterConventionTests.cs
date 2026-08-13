using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class ApiAdapterConventionTests
{
    private static readonly Regex MappedResponseVariableRegex = new(
        @"\.Map\(\s*(?<variable>[a-z][A-Za-z0-9_]*)\s*=>",
        RegexOptions.CultureInvariant);

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

    [Fact(DisplayName = "Razor components constructor-inject API adapters")]
    public void RazorComponents_Should_ConstructorInjectApiAdapters()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        string[] apiAdapterNames = Directory
            .EnumerateFiles(modulesRoot, "*Api.cs", SearchOption.AllDirectories)
            .Select(static path => Path.GetFileNameWithoutExtension(path)!)
            .ToArray();
        List<string> violations = [];

        foreach (string componentPath in Directory.EnumerateFiles(
                     modulesRoot,
                     "*.razor.cs",
                     SearchOption.AllDirectories))
        {
            string source = File.ReadAllText(componentPath);
            string componentName = Path.GetFileNameWithoutExtension(
                Path.GetFileNameWithoutExtension(componentPath));
            Match classDeclaration = Regex.Match(
                source,
                $@"\bpartial\s+class\s+{Regex.Escape(componentName)}\s*" +
                @"\((?<parameters>[^)]*)\)",
                RegexOptions.CultureInvariant | RegexOptions.Singleline);
            string parameters = classDeclaration.Success
                ? classDeclaration.Groups["parameters"].Value
                : string.Empty;

            foreach (string apiAdapterName in apiAdapterNames.Where(
                         apiAdapterName => Regex.IsMatch(
                             source,
                             $@"\b{Regex.Escape(apiAdapterName)}\b",
                             RegexOptions.CultureInvariant)))
            {
                if (!Regex.IsMatch(
                        parameters,
                        $@"\b{Regex.Escape(apiAdapterName)}\s+" +
                        @"[A-Za-z_][A-Za-z0-9_]*\b",
                        RegexOptions.CultureInvariant))
                {
                    violations.Add(
                        $"{Path.GetRelativePath(repositoryRoot, componentPath)}: " +
                        $"does not constructor-inject '{apiAdapterName}'");
                }
            }
        }

        violations.ShouldBeEmpty();
    }
}
