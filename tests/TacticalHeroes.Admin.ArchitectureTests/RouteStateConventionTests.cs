namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class RouteStateConventionTests
{
    [Fact(DisplayName = "Paged lists expose pagination as query parameters")]
    public void ListPages_Should_UseQueryParameters_When_ListStateIsDefined()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        IReadOnlyDictionary<string, string[]> expectedQueries =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["src/TacticalHeroes.Admin.Shared/Ui/MudPagedListComponentBase.cs"] =
                    ["page", "pageSize"],
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
