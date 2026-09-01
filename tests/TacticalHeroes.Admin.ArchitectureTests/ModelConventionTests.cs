using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class ModelConventionTests
{
    [Fact(DisplayName = "Model folders use classes instead of records")]
    public void ModelSources_Should_UseClasses_When_ModelFoldersAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");

        string[] violations = Directory
            .EnumerateDirectories(sourceRoot, "Model", SearchOption.AllDirectories)
            .Where(static path =>
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase) &&
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase))
            .SelectMany(static path => Directory.EnumerateFiles(
                path,
                "*.cs",
                SearchOption.TopDirectoryOnly))
            .Where(path => RecordDeclarationRegex().IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [GeneratedRegex(
        @"\brecord\s+(?:(?:class|struct)\s+)?[A-Za-z_]",
        RegexOptions.CultureInvariant)]
    private static partial Regex RecordDeclarationRegex();
}
