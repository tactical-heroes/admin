using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class ListPageConventionTests
{
    private static readonly string[] ListPagePaths =
    [
        "src/Modules/TacticalHeroes.Admin.Modules.Compendium/Pages/Factions/FactionsPage.razor",
        "src/Modules/TacticalHeroes.Admin.Modules.Identity/Pages/Roles/RolesPage.razor",
        "src/Modules/TacticalHeroes.Admin.Modules.Identity/Pages/Users/UsersPage.razor",
    ];

    private static readonly Regex IdentifierColumnRegex = new(
        "<MudTh[^>]*>\\s*ID\\s*</MudTh>|DataLabel\\s*=\\s*\"ID\"",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    [Fact(DisplayName = "List widgets use the shared list and row action components")]
    public void ListWidgets_Should_UseSharedComponents_When_AdminListsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string[] listWidgetPaths = GetListWidgetPaths(repositoryRoot);
        List<string> violations = [];

        listWidgetPaths.ShouldNotBeEmpty();

        foreach (string relativePath in listWidgetPaths)
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

    [Fact(DisplayName = "List widgets do not expose identifier columns")]
    public void ListWidgets_Should_NotExposeIdentifiers_When_AdminListsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string[] listWidgetPaths = GetListWidgetPaths(repositoryRoot);
        string[] violations = listWidgetPaths
            .Where(relativePath => IdentifierColumnRegex.IsMatch(
                ReadSource(repositoryRoot, relativePath)))
            .ToArray();

        listWidgetPaths.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "List widgets bind load errors instead of passing a literal")]
    public void ListWidgets_Should_BindLoadErrors_When_AdminListsAreScanned()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        string[] listWidgetPaths = GetListWidgetPaths(repositoryRoot);
        string[] violations = listWidgetPaths
            .Where(relativePath => !ReadSource(repositoryRoot, relativePath)
                .Contains("LoadError=\"@LoadError\"", StringComparison.Ordinal))
            .ToArray();

        listWidgetPaths.ShouldNotBeEmpty();
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

    private static string[] GetListWidgetPaths(string repositoryRoot)
    {
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Modules");

        return Directory
            .EnumerateFiles(
                modulesRoot,
                "*ListWidget.razor",
                SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(repositoryRoot, path)
                .Replace(Path.DirectorySeparatorChar, '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }
}
