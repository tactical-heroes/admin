using System.Xml.Linq;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class ProjectReferenceDependencyTests
{
    private static readonly IReadOnlyDictionary<string, string[]> AllowedReferences =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["TacticalHeroes.Admin.Api"] = [],
            ["TacticalHeroes.Admin.Shared"] = [],
            ["TacticalHeroes.Admin.Modules.Compendium"] =
            [
                "TacticalHeroes.Admin.Api",
                "TacticalHeroes.Admin.Shared",
            ],
            ["TacticalHeroes.Admin.Modules.Identity"] =
            [
                "TacticalHeroes.Admin.Api",
                "TacticalHeroes.Admin.Shared",
            ],
            ["TacticalHeroes.Admin.Client"] =
            [
                "TacticalHeroes.Admin.Api",
                "TacticalHeroes.Admin.Modules.Compendium",
                "TacticalHeroes.Admin.Modules.Identity",
                "TacticalHeroes.Admin.Shared",
            ],
            ["TacticalHeroes.Admin"] =
            [
                "TacticalHeroes.Admin.Client",
                "TacticalHeroes.Admin.Modules.Compendium",
                "TacticalHeroes.Admin.Modules.Identity",
                "TacticalHeroes.Admin.Shared",
            ],
        };

    [Fact(DisplayName = "Production projects follow the allowed dependency graph")]
    public void ProjectReferences_Should_MatchAllowedDependencies_When_ProductionProjectsAreLoaded()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        Dictionary<string, string[]> actualReferences = Directory
            .EnumerateFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories)
            .ToDictionary(
                static path => Path.GetFileNameWithoutExtension(path)!,
                GetProjectReferences,
                StringComparer.Ordinal);

        actualReferences.Keys.Order().ShouldBe(AllowedReferences.Keys.Order());

        foreach ((string project, string[] expectedReferences) in AllowedReferences)
        {
            actualReferences[project].Order().ShouldBe(expectedReferences.Order());
        }
    }

    [Fact(DisplayName = "Modules do not reference other module projects")]
    public void ModuleReferences_Should_NotContainModules_When_ModuleProjectsAreLoaded()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        List<string> violations = [];

        foreach (string projectPath in Directory.EnumerateFiles(
                     modulesRoot,
                     "*.csproj",
                     SearchOption.AllDirectories))
        {
            violations.AddRange(GetProjectReferences(projectPath)
                .Where(static reference => reference.StartsWith(
                    "TacticalHeroes.Admin.Modules.",
                    StringComparison.Ordinal))
                .Select(reference => $"{Path.GetFileName(projectPath)} -> {reference}"));
        }

        violations.ShouldBeEmpty();
    }

    private static string[] GetProjectReferences(string projectPath)
    {
        return [.. XDocument
            .Load(projectPath)
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(static path => Path.GetFileNameWithoutExtension(path!.Replace('\\', '/')))];
    }
}
