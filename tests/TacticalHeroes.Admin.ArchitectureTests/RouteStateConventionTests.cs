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
                ["src/Modules/TacticalHeroes.Admin.Modules.Compendium/Pages/Factions/FactionsPage.razor.cs"] =
                    ["page", "pageSize"],
                ["src/Modules/TacticalHeroes.Admin.Modules.Identity/Pages/Roles/RolesPage.razor.cs"] =
                    ["page", "pageSize"],
                ["src/Modules/TacticalHeroes.Admin.Modules.Identity/Pages/Users/UsersPage.razor.cs"] =
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
