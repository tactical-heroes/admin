using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class ListPageConventionTests
{
    private static readonly string[] ListPagePaths =
    [
        "src/Modules/TacticalHeroes.Admin.Modules.Compendium/Pages/FactionListPage/Ui/FactionListPage.razor",
        "src/Modules/TacticalHeroes.Admin.Modules.Identity/Pages/RoleListPage/Ui/RoleListPage.razor",
        "src/Modules/TacticalHeroes.Admin.Modules.Identity/Pages/UserListPage/Ui/UserListPage.razor",
    ];

    private static readonly string[] ListSurfacePaths =
    [
        "src/Modules/TacticalHeroes.Admin.Modules.Compendium/Pages/FactionListPage/Ui/FactionListPage.razor",
        "src/Modules/TacticalHeroes.Admin.Modules.Identity/Pages/RoleListPage/Ui/RoleListPage.razor",
        "src/Modules/TacticalHeroes.Admin.Modules.Identity/Pages/UserListPage/Ui/UserListPage.razor",
    ];

    [GeneratedRegex(
        "<MudTh[^>]*>\\s*ID\\s*</MudTh>|DataLabel\\s*=\\s*\"ID\"",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex IdentifierColumnRegex();

    [Fact(DisplayName = "ListSurfaces should use shared components when admin lists are scanned")]
    public void ListSurfaces_Should_UseSharedComponents_When_AdminListsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        List<string> violations = [];

        foreach (string relativePath in ListSurfacePaths)
        {
            string source = ReadSource(repositoryRoot, relativePath);

            if (!source.Contains("<EntityList", StringComparison.Ordinal) ||
                !source.Contains("<EntityRowActions", StringComparison.Ordinal) ||
                source.Contains("<MudTable", StringComparison.Ordinal))
            {
                violations.Add(relativePath);
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "ListSurfaces should not expose identifiers when admin lists are scanned")]
    public void ListSurfaces_Should_NotExposeIdentifiers_When_AdminListsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string[] violations = [.. ListSurfacePaths
            .Where(relativePath => IdentifierColumnRegex().IsMatch(
                ReadSource(repositoryRoot, relativePath)))];

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "ListSurfaces should bind load errors when admin lists are scanned")]
    public void ListSurfaces_Should_BindLoadErrors_When_AdminListsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string[] violations = [.. ListSurfacePaths
            .Where(relativePath => !ReadSource(repositoryRoot, relativePath)
                .Contains("LoadError=", StringComparison.Ordinal))];

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "ListPages should expose header and create action when admin lists are scanned")]
    public void ListPages_Should_ExposeHeaderAndCreateAction_When_AdminListsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        List<string> violations = [];

        foreach (string relativePath in ListPagePaths)
        {
            string source = ReadSource(repositoryRoot, relativePath);

            if (!source.Contains("<PageHeader", StringComparison.Ordinal) ||
                !source.Contains("Subtitle=", StringComparison.Ordinal) ||
                !source.Contains("<Actions>", StringComparison.Ordinal))
            {
                violations.Add(relativePath);
            }
        }

        violations.ShouldBeEmpty();
    }

    private static string ReadSource(string repositoryRoot, string relativePath)
    {
        return File.ReadAllText(Path.Combine(
            repositoryRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }
}
