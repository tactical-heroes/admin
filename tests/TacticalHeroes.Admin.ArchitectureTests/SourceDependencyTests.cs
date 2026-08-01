using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class SourceDependencyTests
{
    private static readonly IReadOnlyDictionary<string, int> LayerRanks =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["Entities"] = 1,
            ["Features"] = 2,
            ["Widgets"] = 3,
            ["Pages"] = 4,
        };

    [Fact(DisplayName = "Identity module follows FSD dependency direction")]
    public void IdentitySources_Should_NotDependOnHigherLayers_When_ModuleIsScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string moduleRoot = Path.Combine(
            repositoryRoot,
            "src",
            "Modules",
            "Identity",
            "TacticalHeroes.Admin.Modules.Identity");
        List<string> violations = [];

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

            foreach (Match match in IdentityLayerReferenceRegex().Matches(source))
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

    [GeneratedRegex(
        @"TacticalHeroes\.Admin\.Modules\.Identity\.(Pages|Widgets|Features|Entities)",
        RegexOptions.CultureInvariant)]
    private static partial Regex IdentityLayerReferenceRegex();
}
