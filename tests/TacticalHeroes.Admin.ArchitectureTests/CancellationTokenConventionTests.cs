using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class CancellationTokenConventionTests
{
    private static readonly Regex DefaultCancellationTokenRegex = new(
        @"\bCancellationToken\s+[A-Za-z_]\w*\s*=\s*default\b",
        RegexOptions.CultureInvariant);

    [Fact(DisplayName = "Cancellation tokens are required parameters")]
    public void CancellationTokens_Should_NotHaveDefaultValues_When_SourcesAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string[] sourceRoots =
        [
            Path.Combine(repositoryRoot, "src"),
            Path.Combine(repositoryRoot, "tests"),
        ];
        List<string> violations = [];

        foreach (string sourcePath in sourceRoots.SelectMany(EnumerateSourceFiles))
        {
            if (DefaultCancellationTokenRegex.IsMatch(File.ReadAllText(sourcePath)))
            {
                violations.Add(Path.GetRelativePath(repositoryRoot, sourcePath));
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
