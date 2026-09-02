using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class RazorArtifactConventionTests
{
    [GeneratedRegex(
        "@(?:code|functions)\\s*\\{|@inject\\s|<style(?:\\s|>)",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex InlineCodeOrStyleRegex();

    [GeneratedRegex(
        "<EditForm(?:\\s|>)",
        RegexOptions.CultureInvariant)]
    private static partial Regex EditFormRegex();

    [GeneratedRegex(
        @"\[(?:global::)?(?:[A-Za-z_][A-Za-z0-9_]*\.)*Inject(?:Attribute)?\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex PropertyInjectionRegex();

    [Fact(DisplayName = "Razor markup keeps code and styles in companion files")]
    public void RazorMarkup_Should_NotContainCodeOrStyles_When_SourceIsScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        string[] violations = [.. EnumerateFiles(sourceRoot, "*.razor")
            .Where(path => InlineCodeOrStyleRegex().IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))];

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Razor import blocks are separated from directives and markup")]
    public void RazorImports_Should_UseSeparateBlocks_When_SourceIsScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        List<string> violations = [];

        foreach (string path in EnumerateFiles(sourceRoot, "*.razor"))
        {
            string[] lines = File.ReadAllLines(path);

            for (int index = 0; index < lines.Length; index++)
            {
                if (!IsUsingDirective(lines[index]))
                {
                    continue;
                }

                string relativePath = Path.GetRelativePath(repositoryRoot, path);

                if (index > 0 &&
                    !IsUsingDirective(lines[index - 1]) &&
                    !string.IsNullOrWhiteSpace(lines[index - 1]))
                {
                    violations.Add($"{relativePath}:{index + 1}: add a blank line before the @using block");
                }

                if (index + 1 < lines.Length &&
                    !IsUsingDirective(lines[index + 1]) &&
                    !string.IsNullOrWhiteSpace(lines[index + 1]))
                {
                    violations.Add($"{relativePath}:{index + 1}: add a blank line after the @using block");
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Razor companion files belong to an existing component")]
    public void RazorCompanions_Should_HaveComponent_When_CompanionsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        List<string> violations = [];

        foreach (string codeBehindPath in EnumerateFiles(sourceRoot, "*.razor.cs"))
        {
            string componentPath = codeBehindPath[..^".cs".Length];
            string componentName = Path.GetFileNameWithoutExtension(componentPath);
            string codeBehind = File.ReadAllText(codeBehindPath);

            if (!File.Exists(componentPath) ||
                !Regex.IsMatch(
                    codeBehind,
                    $@"\bpartial\s+class\s+{Regex.Escape(componentName)}\b",
                    RegexOptions.CultureInvariant))
            {
                violations.Add(Path.GetRelativePath(repositoryRoot, codeBehindPath));
            }
        }

        foreach (string cssPath in EnumerateFiles(sourceRoot, "*.razor.css"))
        {
            string componentPath = cssPath[..^".css".Length];

            if (!File.Exists(componentPath))
            {
                violations.Add(Path.GetRelativePath(repositoryRoot, cssPath));
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Razor components use constructor injection")]
    public void RazorComponents_Should_NotUsePropertyInjection()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        string[] violations = [.. EnumerateFiles(sourceRoot, "*.cs")
            .Where(path => PropertyInjectionRegex().IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))];

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Razor forms use MudForm instead of EditForm")]
    public void RazorForms_Should_UseMudForm_When_FormsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        string[] violations = [.. EnumerateFiles(sourceRoot, "*.razor")
            .Where(path => EditFormRegex().IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))];

        violations.ShouldBeEmpty();
    }

    private static IEnumerable<string> EnumerateFiles(string root, string pattern)
    {
        return Directory
            .EnumerateFiles(root, pattern, SearchOption.AllDirectories)
            .Where(static path =>
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase) &&
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUsingDirective(string line)
    {
        return line.StartsWith("@using ", StringComparison.Ordinal);
    }
}
