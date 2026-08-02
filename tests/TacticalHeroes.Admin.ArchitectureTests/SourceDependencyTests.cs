using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class SourceDependencyTests
{
    private static readonly Dictionary<string, int> LayerRanks =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["Entities"] = 1,
            ["Features"] = 2,
            ["Widgets"] = 3,
            ["Pages"] = 4,
        };

    [Fact(DisplayName = "Modules follow FSD dependency direction")]
    public void ModuleSources_Should_NotDependOnHigherLayers_When_ModulesAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        List<string> violations = [];

        foreach (string moduleRoot in Directory.EnumerateDirectories(
                     modulesRoot,
                     "TacticalHeroes.Admin.Modules.*"))
        {
            string moduleNamespace = Path.GetFileName(moduleRoot);
            Regex layerReferenceRegex = new(
                $@"{Regex.Escape(moduleNamespace)}\.(Pages|Widgets|Features|Entities)",
                RegexOptions.CultureInvariant);

            foreach (string sourcePath in EnumerateSourceFiles(moduleRoot))
            {
                string sourceLayer = Path
                    .GetRelativePath(moduleRoot, sourcePath)
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];

                if (!LayerRanks.TryGetValue(sourceLayer, out int sourceRank))
                {
                    continue;
                }

                string source = File.ReadAllText(sourcePath);

                foreach (Match match in layerReferenceRegex.Matches(source))
                {
                    string targetLayer = match.Groups[1].Value;

                    if (LayerRanks[targetLayer] > sourceRank)
                    {
                        violations.Add(
                            $"{Path.GetRelativePath(repositoryRoot, sourcePath)}: " +
                            $"{sourceLayer} -> {targetLayer}");
                    }
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Foundation projects do not depend on application layers")]
    public void FoundationSources_Should_NotDependOnHigherProjects_When_SourcesAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string[] foundationRoots =
        [
            Path.Combine(repositoryRoot, "src", "TacticalHeroes.Admin.Api"),
            Path.Combine(repositoryRoot, "src", "TacticalHeroes.Admin.Shared"),
        ];
        string[] forbiddenNamespaces =
        [
            "TacticalHeroes.Admin.Client",
            "TacticalHeroes.Admin.Modules",
        ];
        List<string> violations = [];

        foreach (string foundationRoot in foundationRoots)
        {
            foreach (string sourcePath in EnumerateSourceFiles(foundationRoot))
            {
                string source = File.ReadAllText(sourcePath);

                foreach (string forbiddenNamespace in forbiddenNamespaces)
                {
                    if (source.Contains(forbiddenNamespace, StringComparison.Ordinal))
                    {
                        violations.Add(
                            $"{Path.GetRelativePath(repositoryRoot, sourcePath)} -> " +
                            forbiddenNamespace);
                    }
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Application components use route contracts")]
    public void ComponentRoutes_Should_UseContracts_When_ApplicationComponentsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string[] applicationRoots =
        [
            Path.Combine(repositoryRoot, "src", "TacticalHeroes.Admin.Client"),
            Path.Combine(repositoryRoot, "src", "Modules"),
        ];
        Regex rawRouteRegex = new(
            "@page\\s+\"|(?:Href|href|action)\\s*=\\s*\"/|NavigateTo\\(\\s*\"/",
            RegexOptions.CultureInvariant);
        List<string> violations = [];

        foreach (string applicationRoot in applicationRoots)
        {
            foreach (string sourcePath in Directory.EnumerateFiles(
                         applicationRoot,
                         "*.razor",
                         SearchOption.AllDirectories))
            {
                if (sourcePath.Contains(
                        $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                        StringComparison.OrdinalIgnoreCase) ||
                    sourcePath.Contains(
                        $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (rawRouteRegex.IsMatch(File.ReadAllText(sourcePath)))
                {
                    violations.Add(Path.GetRelativePath(repositoryRoot, sourcePath));
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    private static IEnumerable<string> EnumerateSourceFiles(string root)
    {
        return Directory
            .EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(static path =>
                (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                 path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)) &&
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase) &&
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase));
    }
}
