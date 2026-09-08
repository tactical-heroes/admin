using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class EnumerationConventionTests
{
    [Fact(DisplayName = "Enumeration members use explicit numeric values")]
    public void EnumerationMembers_Should_HaveExplicitNumericValues_When_SourceIsScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        EnumerationMember[] members = [.. EnumerateMembers(sourceRoot)];

        string[] violations =
        [
            .. members
                .Where(static member => string.IsNullOrWhiteSpace(member.NumericValue))
                .Select(member => FormatViolation(repositoryRoot, member)),
        ];

        members.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Enumeration members have English display names")]
    public void EnumerationMembers_Should_HaveEnglishDisplayNames_When_SourceIsScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        EnumerationMember[] members = [.. EnumerateMembers(sourceRoot)];
        List<string> violations = [];

        foreach (EnumerationMember member in members)
        {
            Match displayNameMatch = DisplayNameRegex().Match(member.Attributes);
            if (!displayNameMatch.Success ||
                !IsEnglishDisplayName(displayNameMatch.Groups["name"].Value))
            {
                violations.Add(FormatViolation(repositoryRoot, member));
            }
        }

        members.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Enumeration scanner recognizes valid explicit initializers")]
    [InlineData("-1")]
    [InlineData("0x10")]
    [InlineData("Read | Write")]
    public void EnumerationScanner_Should_ReadValue_When_InitializerIsValid(string initializer)
    {
        string source = $$"""
            public enum TestEnumeration
            {
                [Display(Name = "Test value")]
                Value = {{initializer}},
            }
            """;

        EnumerationMember member = EnumerateMembers("test.cs", source).ShouldHaveSingleItem();

        member.NumericValue.ShouldBe(initializer);
    }

    private static IEnumerable<EnumerationMember> EnumerateMembers(string sourceRoot)
    {
        foreach (string sourcePath in EnumerateSourceFiles(sourceRoot))
        {
            string source = File.ReadAllText(sourcePath);

            foreach (EnumerationMember member in EnumerateMembers(sourcePath, source))
            {
                yield return member;
            }
        }
    }

    private static IEnumerable<EnumerationMember> EnumerateMembers(
        string sourcePath,
        string source)
    {
        foreach (Match enumerationMatch in EnumerationRegex().Matches(source))
        {
            string enumerationName = enumerationMatch.Groups["name"].Value;
            string body = enumerationMatch.Groups["body"].Value;

            foreach (Match memberMatch in EnumerationMemberRegex().Matches(body))
            {
                yield return new EnumerationMember(
                    sourcePath,
                    enumerationName,
                    memberMatch.Groups["name"].Value,
                    memberMatch.Groups["value"].Value.Trim(),
                    memberMatch.Groups["attributes"].Value);
            }
        }
    }

    private static IEnumerable<string> EnumerateSourceFiles(string sourceRoot)
    {
        return Directory
            .EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(static path =>
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase) &&
                !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsEnglishDisplayName(string displayName)
    {
        return displayName.Any(char.IsAsciiLetter) &&
               displayName.All(static character =>
                   char.IsAscii(character) && !char.IsControl(character));
    }

    private static string FormatViolation(
        string repositoryRoot,
        EnumerationMember member)
    {
        return $"{Path.GetRelativePath(repositoryRoot, member.SourcePath)}: " +
               $"{member.EnumerationName}.{member.Name}";
    }

    [GeneratedRegex(
        @"\benum\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)[^\{]*\{(?<body>[^\}]*)\}",
        RegexOptions.CultureInvariant | RegexOptions.Singleline)]
    private static partial Regex EnumerationRegex();

    [GeneratedRegex(
        @"(?m)(?<attributes>(?:^[ \t]*\[[^\]\r\n]+\][ \t]*\r?\n)*)" +
        @"^[ \t]*(?<name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*" +
        @"(?:=[ \t]*(?<value>.+?))?[ \t]*,?[ \t]*(?://.*)?$",
        RegexOptions.CultureInvariant)]
    private static partial Regex EnumerationMemberRegex();

    [GeneratedRegex(
        @"\bDisplay(?:Attribute)?\s*\(\s*Name\s*=\s*""(?<name>(?:\\.|[^""])*)""\s*\)",
        RegexOptions.CultureInvariant)]
    private static partial Regex DisplayNameRegex();

    private sealed record EnumerationMember(
        string SourcePath,
        string EnumerationName,
        string Name,
        string NumericValue,
        string Attributes);
}
