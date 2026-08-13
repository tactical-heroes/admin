namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class RouteStateConventionTests
{
    [Fact(DisplayName = "Paged lists expose page and filters as query parameters")]
    public void ListPages_Should_UseQueryParameters_When_ListStateIsDefined()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        IReadOnlyDictionary<string, string[]> expectedQueries =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["src/Modules/TacticalHeroes.Admin.Modules.Compendium/Pages/FactionListPage/Ui/FactionListPage.razor.cs"] =
                    ["page", "pageSize"],
                ["src/Modules/TacticalHeroes.Admin.Modules.Identity/Pages/RoleListPage/Ui/RoleListPage.razor.cs"] =
                    ["page", "pageSize"],
                ["src/Modules/TacticalHeroes.Admin.Modules.Identity/Pages/UserListPage/Ui/UserListPage.razor.cs"] =
                    ["page", "email", "pageSize"],
            };
        List<string> violations = [];

        foreach ((string relativePath, string[] queryNames) in expectedQueries)
        {
            string sourcePath = Path.Combine(
                repositoryRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            string source = File.ReadAllText(sourcePath);

            foreach (string queryName in queryNames)
            {
                if (!source.Contains(
                        $"[SupplyParameterFromQuery(Name = \"{queryName}\")]",
                        StringComparison.Ordinal))
                {
                    violations.Add($"{relativePath}: {queryName}");
                }
            }
        }

        violations.ShouldBeEmpty();
    }
}
