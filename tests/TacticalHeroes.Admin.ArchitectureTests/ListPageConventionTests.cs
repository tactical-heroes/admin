using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class ListPageConventionTests
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

    private static readonly Regex IdentifierColumnRegex = new(
        "<MudTh[^>]*>\\s*ID\\s*</MudTh>|DataLabel\\s*=\\s*\"ID\"",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    [Fact(DisplayName = "List surfaces use the shared list and row action components")]
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

    [Fact(DisplayName = "List surfaces do not expose identifier columns")]
    public void ListSurfaces_Should_NotExposeIdentifiers_When_AdminListsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string[] violations = ListSurfacePaths
            .Where(relativePath => IdentifierColumnRegex.IsMatch(
                ReadSource(repositoryRoot, relativePath)))
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "List surfaces bind load errors instead of passing a literal")]
    public void ListSurfaces_Should_BindLoadErrors_When_AdminListsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string[] violations = ListSurfacePaths
            .Where(relativePath => !ReadSource(repositoryRoot, relativePath)
                .Contains("LoadError=", StringComparison.Ordinal))
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "List pages expose a page header and create action")]
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
