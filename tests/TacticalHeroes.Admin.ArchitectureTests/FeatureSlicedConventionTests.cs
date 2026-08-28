using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class FeatureSlicedConventionTests
{
    private static readonly IReadOnlyDictionary<string, int> ClientLayerRanks =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["Entities"] = 1,
            ["Features"] = 2,
            ["Widgets"] = 3,
            ["Pages"] = 4,
            ["App"] = 5,
        };

    private static readonly IReadOnlyDictionary<string, int> ModuleLayerRanks =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["Entities"] = 1,
            ["Features"] = 2,
            ["Widgets"] = 3,
            ["Pages"] = 4,
        };

    [Fact(DisplayName = "Client and modules use only FSD top-level layers")]
    public void SourceFolders_Should_UseKnownLayers_When_ApplicationRootsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        List<string> violations = [];
        string clientRoot = Path.Combine(repositoryRoot, "src", "TacticalHeroes.Admin.Client");

        AddUnknownDirectories(clientRoot, ClientLayerRanks.Keys, violations);
        AddUnknownRootSources(
            clientRoot,
            static fileName => fileName is "Program.cs" or "_Imports.razor",
            violations);

        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        foreach (string moduleRoot in Directory.EnumerateDirectories(
                     modulesRoot,
                     "TacticalHeroes.Admin.Modules.*"))
        {
            AddUnknownDirectories(moduleRoot, ModuleLayerRanks.Keys, violations);
            AddUnknownRootSources(
                moduleRoot,
                static fileName =>
                    fileName == "_Imports.razor" ||
                    fileName.EndsWith("AdminModule.cs", StringComparison.Ordinal) ||
                    fileName.EndsWith("Routes.cs", StringComparison.Ordinal),
                violations);
        }

        violations
            .Select(path => Path.GetRelativePath(repositoryRoot, path))
            .ShouldBeEmpty();
    }

    [Fact(DisplayName = "FSD source namespaces match their folders")]
    public void SourceNamespaces_Should_MatchFolders_When_FsdSourcesAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        List<string> violations = [];
        string clientRoot = Path.Combine(repositoryRoot, "src", "TacticalHeroes.Admin.Client");

        AddNamespaceViolations(
            clientRoot,
            "TacticalHeroes.Admin.Client",
            ClientLayerRanks.Keys,
            violations);

        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        foreach (string moduleRoot in Directory.EnumerateDirectories(
                     modulesRoot,
                     "TacticalHeroes.Admin.Modules.*"))
        {
            AddNamespaceViolations(
                moduleRoot,
                Path.GetFileName(moduleRoot),
                ModuleLayerRanks.Keys,
                violations);
        }

        violations
            .Select(path => Path.GetRelativePath(repositoryRoot, path))
            .ShouldBeEmpty();
    }

    [Fact(DisplayName = "FSD layers depend only on lower layers")]
    public void SourceDependencies_Should_FollowLayerDirection_When_FsdSourcesAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        List<string> violations = [];
        string clientRoot = Path.Combine(repositoryRoot, "src", "TacticalHeroes.Admin.Client");

        AddLayerDependencyViolations(
            clientRoot,
            "TacticalHeroes.Admin.Client",
            ClientLayerRanks,
            violations);

        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        foreach (string moduleRoot in Directory.EnumerateDirectories(
                     modulesRoot,
                     "TacticalHeroes.Admin.Modules.*"))
        {
            AddLayerDependencyViolations(
                moduleRoot,
                Path.GetFileName(moduleRoot),
                ModuleLayerRanks,
                violations);
        }

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Modules do not depend on other modules")]
    public void ModuleSources_Should_NotReferenceOtherModules_When_ModulesAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        Regex moduleReferenceRegex = ModuleReferenceRegex();
        List<string> violations = [];

        foreach (string moduleRoot in Directory.EnumerateDirectories(
                     modulesRoot,
                     "TacticalHeroes.Admin.Modules.*"))
        {
            string currentModule = Path.GetFileName(moduleRoot).Split('.')[^1];

            foreach (string sourcePath in EnumerateSourceFiles(moduleRoot))
            {
                foreach (Match match in moduleReferenceRegex.Matches(File.ReadAllText(sourcePath)))
                {
                    string referencedModule = match.Groups["module"].Value;
                    if (!string.Equals(
                            referencedModule,
                            currentModule,
                            StringComparison.Ordinal))
                    {
                        violations.Add(
                            $"{Path.GetRelativePath(repositoryRoot, sourcePath)} -> " +
                            referencedModule);
                    }
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Page slices do not depend on sibling page slices")]
    public void PageSources_Should_NotReferenceSiblingSlices_When_PagesAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");
        List<string> violations = [];

        foreach (string moduleRoot in Directory.EnumerateDirectories(
                     modulesRoot,
                     "TacticalHeroes.Admin.Modules.*"))
        {
            string pagesRoot = Path.Combine(moduleRoot, "Pages");
            if (!Directory.Exists(pagesRoot))
            {
                continue;
            }

            string rootNamespace = Path.GetFileName(moduleRoot);
            Regex pageReferenceRegex = new(
                $@"\b{Regex.Escape(rootNamespace)}\.Pages\.(?<slice>[A-Za-z0-9_]+)\.",
                RegexOptions.CultureInvariant);

            foreach (string sourcePath in EnumerateSourceFiles(pagesRoot))
            {
                string sourceSlice = Path
                    .GetRelativePath(pagesRoot, sourcePath)
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];

                foreach (Match match in pageReferenceRegex.Matches(
                             File.ReadAllText(sourcePath)))
                {
                    string targetSlice = match.Groups["slice"].Value;
                    if (!string.Equals(sourceSlice, targetSlice, StringComparison.Ordinal))
                    {
                        violations.Add(
                            $"{Path.GetRelativePath(repositoryRoot, sourcePath)}: " +
                            $"{sourceSlice} -> {targetSlice}");
                    }
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    private static void AddUnknownDirectories(
        string root,
        IEnumerable<string> layers,
        List<string> violations)
    {
        HashSet<string> allowedDirectories = new(layers, StringComparer.Ordinal)
        {
            "bin",
            "obj",
            "Properties",
            "wwwroot",
        };

        violations.AddRange(Directory
            .EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly)
            .Where(path => !allowedDirectories.Contains(Path.GetFileName(path)))
            .Where(static path => Directory.EnumerateFiles(
                path,
                "*.*",
                SearchOption.AllDirectories).Any()));
    }

    private static void AddUnknownRootSources(
        string root,
        Func<string, bool> isAllowed,
        List<string> violations)
    {
        violations.AddRange(Directory
            .EnumerateFiles(root, "*.*", SearchOption.TopDirectoryOnly)
            .Where(static path =>
                path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            .Where(path => !isAllowed(Path.GetFileName(path))));
    }

    private static void AddNamespaceViolations(
        string root,
        string rootNamespace,
        IEnumerable<string> layers,
        List<string> violations)
    {
        HashSet<string> knownLayers = new(layers, StringComparer.Ordinal);
        Regex namespaceRegex = NamespaceRegex();

        foreach (string sourcePath in EnumerateSourceFiles(root))
        {
            string relativePath = Path.GetRelativePath(root, sourcePath);
            string sourceLayer = relativePath
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
            if (!knownLayers.Contains(sourceLayer))
            {
                continue;
            }

            if (sourcePath.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            {
                if (File.ReadAllText(sourcePath).Contains("@namespace", StringComparison.Ordinal))
                {
                    violations.Add(sourcePath);
                }

                continue;
            }

            string? relativeDirectory = Path.GetDirectoryName(relativePath);
            string expectedNamespace = relativeDirectory is null
                ? rootNamespace
                : $"{rootNamespace}.{relativeDirectory.Replace(Path.DirectorySeparatorChar, '.')}";
            Match namespaceMatch = namespaceRegex.Match(File.ReadAllText(sourcePath));

            if (!namespaceMatch.Success ||
                !string.Equals(
                    namespaceMatch.Groups["namespace"].Value,
                    expectedNamespace,
                    StringComparison.Ordinal))
            {
                violations.Add(sourcePath);
            }
        }
    }

    private static void AddLayerDependencyViolations(
        string root,
        string rootNamespace,
        IReadOnlyDictionary<string, int> layerRanks,
        List<string> violations)
    {
        string layers = string.Join('|', layerRanks.Keys.Select(Regex.Escape));
        Regex layerReferenceRegex = new(
            $@"(?<![A-Za-z0-9_.])(?:{Regex.Escape(rootNamespace)}\.)?(?<layer>{layers})\.",
            RegexOptions.CultureInvariant);

        foreach (string sourcePath in EnumerateSourceFiles(root))
        {
            string sourceLayer = Path
                .GetRelativePath(root, sourcePath)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
            if (!layerRanks.TryGetValue(sourceLayer, out int sourceRank))
            {
                continue;
            }

            foreach (Match match in layerReferenceRegex.Matches(File.ReadAllText(sourcePath)))
            {
                string targetLayer = match.Groups["layer"].Value;
                if (layerRanks[targetLayer] > sourceRank)
                {
                    violations.Add(
                        $"{Path.GetRelativePath(RepositoryPaths.FindRoot(), sourcePath)}: " +
                        $"{sourceLayer} -> {targetLayer}");
                }
            }
        }
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
        @"TacticalHeroes\.Admin\.Modules\.(?<module>[A-Za-z0-9_]+)",
        RegexOptions.CultureInvariant)]
    private static partial Regex ModuleReferenceRegex();

    [GeneratedRegex(
        @"\bnamespace\s+(?<namespace>[A-Za-z0-9_.]+)\s*[;{]",
        RegexOptions.CultureInvariant)]
    private static partial Regex NamespaceRegex();
}
