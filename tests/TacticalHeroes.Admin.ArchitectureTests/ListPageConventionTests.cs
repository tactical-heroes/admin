using System.Text.RegularExpressions;

namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed partial class ListPageConventionTests
{
    [Fact(DisplayName = "ListPages should use shared components when admin lists are scanned")]
    public void ListPages_Should_UseSharedComponents_When_AdminListsAreScanned()
    {
        PageConventionSource[] pages = PageConventionSource.Discover("*ListPage.razor");
        string[] violations = [.. pages
            .Where(page => !UsesSharedComponents(page.Markup))
            .Select(page => page.RelativePath)];

        pages.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "ListPages should not expose identifiers when admin lists are scanned")]
    public void ListPages_Should_NotExposeIdentifiers_When_AdminListsAreScanned()
    {
        PageConventionSource[] pages = PageConventionSource.Discover("*ListPage.razor");
        string[] violations = [.. pages
            .Where(page => IdentifierColumnRegex().IsMatch(page.Markup))
            .Select(page => page.RelativePath)];

        pages.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "ListPages should bind load errors when admin lists are scanned")]
    public void ListPages_Should_BindLoadErrors_When_AdminListsAreScanned()
    {
        PageConventionSource[] pages = PageConventionSource.Discover("*ListPage.razor");
        string[] violations = [.. pages
            .Where(page => !PageConventionSource.Binds(
                PageConventionSource.Element(page.Markup, "EntityList"), "LoadError", "LoadError", explicitExpression: true))
            .Select(page => page.RelativePath)];

        pages.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "ListPages should expose header and create action when admin lists are scanned")]
    public void ListPages_Should_ExposeHeaderAndCreateAction_When_AdminListsAreScanned()
    {
        PageConventionSource[] pages = PageConventionSource.Discover("*ListPage.razor");
        string[] violations = [.. pages
            .Where(page => !HasHeaderAndCreateAction(page.Markup))
            .Select(page => page.RelativePath)];

        pages.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Fact(DisplayName = "ListPages should bind paged list base when admin lists are scanned")]
    public void ListPages_Should_BindPagedListBase_When_AdminListsAreScanned()
    {
        PageConventionSource[] pages = PageConventionSource.Discover("*ListPage.razor");
        string[] violations = [.. pages
            .Where(page => !page.Inherits("Lists.MudPagedListComponentBase") || !BindsListState(page.Markup))
            .Select(page => page.RelativePath)];

        pages.ShouldNotBeEmpty();
        violations.ShouldBeEmpty();
    }

    [Theory(DisplayName = "List markup should reject missing shared composition when markup is inspected")]
    [InlineData("<EntityList><RowTemplate><EntityRowActions /></RowTemplate></EntityList>", true)]
    [InlineData("<EntityList /><EntityRowActions />", false)]
    [InlineData("<EntityList><RowTemplate><EntityRowActions /></RowTemplate></EntityList><MudTable />", false)]
    [InlineData("<EntityList><RowTemplate><EntityRowActions /></RowTemplate></EntityList><table></table>", false)]
    public void ListMarkup_Should_RejectMissingSharedComposition_When_MarkupIsInspected(string markup, bool expected)
    {
        bool valid = UsesSharedComponents(markup);

        valid.ShouldBe(expected);
    }

    [Theory(DisplayName = "List header should require a create link inside actions when markup is inspected")]
    [InlineData("<Actions><MudButton Href=\"@ExampleRoutes.CreateExample\" /></Actions>", true)]
    [InlineData("<Actions><MudButton Href=\"@ExampleRoutes.Examples\" /><MudButton Href=\"@ExampleRoutes.CreateExample\" /></Actions>", true)]
    [InlineData("<Actions />", false)]
    [InlineData("<Actions><MudButton Href=\"@ExampleRoutes.Examples\" /></Actions>", false)]
    [InlineData("<Actions><MudButton Href=\"ExampleRoutes.CreateExample\" /></Actions>", false)]
    [InlineData("<MudButton Href=\"@ExampleRoutes.CreateExample\" />", false)]
    public void ListHeader_Should_RequireACreateLinkInsideActions_When_MarkupIsInspected(string actions, bool expected)
    {
        string markup = "<PageHeader Title=\"Examples\" Subtitle=\"Manage examples\">" + actions + "</PageHeader>";

        HasHeaderAndCreateAction(markup).ShouldBe(expected);
    }

    [Fact(DisplayName = "Page discovery should include new entities and modules when source paths are scanned")]
    public void PageDiscovery_Should_IncludeNewEntitiesAndModules_When_SourcePathsAreScanned()
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("admin-page-conventions-");
        string[] expected =
        [
            "Client/Pages/InventoryListPage/Ui/InventoryListPage.razor",
            "Modules/NewModule/Pages/CreateItemPage/Ui/CreateItemPage.razor",
            "Modules/NewModule/Pages/UpdateItemPage/Ui/UpdateItemPage.razor",
            "Modules/NewModule/Pages/EditItemPage/Ui/EditItemPage.razor"
        ];

        try
        {
            foreach (string relativePath in expected.Concat([
                         "Modules/NewModule/Widgets/OtherListPage.razor",
                         "Modules/NewModule/obj/Pages/GeneratedListPage.razor",
                         "Modules/NewModule/bin/Pages/GeneratedListPage.razor",
                         "Client/Pages/LoginPage/Ui/LoginPage.razor"]))
            {
                string path = Path.Combine(directory.FullName, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, "");
            }

            string[] paths = PageConventionSource.DiscoverPaths(directory.FullName,
                "*ListPage.razor", "Create*Page.razor", "Update*Page.razor", "Edit*Page.razor");

            paths.Select(path => Path.GetRelativePath(directory.FullName, path).Replace('\\', '/'))
                .ShouldBe(expected.Order(StringComparer.Ordinal));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Theory(DisplayName = "Page markup should ignore comments when shared tags are inspected")]
    [InlineData("@* <EntityList LoadError=\"@LoadError\" /> *@", false)]
    [InlineData("<!-- <EntityList LoadError=\"@LoadError\" /> -->", false)]
    [InlineData("<EntityList LoadError=\"LoadError\" />", false)]
    [InlineData("<EntityList LoadError = '@(LoadError)' />", true)]
    [InlineData("<Other LoadError=\"@LoadError\" /><EntityList />", false)]
    [InlineData("<EntityList EmptyText='LoadError=\"@LoadError\"' />", false)]
    [InlineData("@* LoadError=\"wrong\" *@ <EntityList LoadError=\"@LoadError\" />", true)]
    public void PageMarkup_Should_IgnoreComments_When_SharedTagsAreInspected(string markup, bool expected)
    {
        Match list = PageConventionSource.Element(PageConventionSource.WithoutComments(markup), "EntityList");

        PageConventionSource.Binds(list, "LoadError", "LoadError", explicitExpression: true).ShouldBe(expected);
    }

    private static bool UsesSharedComponents(string markup)
    {
        Match list = PageConventionSource.Element(markup, "EntityList");
        Match row = PageConventionSource.Element(list.Groups["content"].Value, "RowTemplate");

        return list.Success && PageConventionSource.Element(row.Groups["content"].Value, "EntityRowActions").Success &&
            !OwnTableRegex().IsMatch(markup);
    }

    private static bool HasHeaderAndCreateAction(string markup)
    {
        Match header = PageConventionSource.Element(markup, "PageHeader");
        Match actions = PageConventionSource.Element(header.Groups["content"].Value, "Actions");

        if (!PageConventionSource.HasHeader(markup))
        {
            return false;
        }

        for (Match button = PageConventionSource.Element(actions.Groups["content"].Value, "MudButton");
             button.Success;
             button = button.NextMatch())
        {
            if (CreateRouteRegex().IsMatch(PageConventionSource.Attribute(button, "Href") ?? ""))
            {
                return true;
            }
        }

        return false;
    }

    private static bool BindsListState(string markup)
    {
        Match list = PageConventionSource.Element(markup, "EntityList");

        return PageConventionSource.Binds(list, "Items", "Page?.Items") &&
            PageConventionSource.Binds(list, "Loading", "IsLoading") &&
            PageConventionSource.Binds(list, "OnRefresh", "LoadPageAsync") &&
            PageConventionSource.Binds(list, "PageNumber", "CurrentPageNumber") &&
            PageConventionSource.Binds(list, "PageSize", "CurrentPageSize") &&
            PageConventionSource.Binds(list, "TotalPages", "TotalPages") &&
            PageConventionSource.Binds(list, "TotalCount", "TotalCount") &&
            PageConventionSource.Binds(list, "OnPageNumberChanged", "ChangePage") &&
            PageConventionSource.Binds(list, "OnPageSizeChanged", "ChangePageSize");
    }

    [GeneratedRegex("<MudTh[^>]*>\\s*ID\\s*</MudTh>|DataLabel\\s*=\\s*[\"']ID[\"']", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex IdentifierColumnRegex();

    [GeneratedRegex("<(?:MudTable|MudDataGrid|table)(?=[\\s/>])", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex OwnTableRegex();

    [GeneratedRegex(@"^@(?:\([\t ]*)?(?:[\w:]+\.)+Create\w+[\t ]*\)?$", RegexOptions.CultureInvariant)]
    private static partial Regex CreateRouteRegex();
}
