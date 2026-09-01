using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class ModelConventionTests
{
    [Fact(DisplayName = "Model folders use property-based classes")]
    public void ModelSources_Should_UsePropertyBasedClasses_When_ModelFoldersAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");

        string[] violations =
        [
            .. Directory
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
                .Where(path => PositionalTypeDeclarationRegex().IsMatch(
                    File.ReadAllText(path)))
                .Select(path => Path.GetRelativePath(repositoryRoot, path)),
        ];

        violations.ShouldBeEmpty();
    }

    [GeneratedRegex(
        @"\brecord\s+(?:(?:class|struct)\s+)?[A-Za-z_]|" +
        @"\bclass\s+[A-Za-z_][A-Za-z0-9_]*(?:<[^>{}\r\n]+>)?\s*\(",
        RegexOptions.CultureInvariant)]
    private static partial Regex PositionalTypeDeclarationRegex();
}
