namespace TacticalHeroes.Admin.ArchitectureTests;

public sealed class RouteStateConventionTests
{
    [Fact(DisplayName = "ListPages should use query parameters when list state is defined")]
    public void ListPages_Should_UseQueryParameters_When_ListStateIsDefined()
    {
        string repositoryRoot = RepositoryPaths.FindRoot();
        IReadOnlyDictionary<string, string[]> expectedQueries =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["src/TacticalHeroes.Admin.Shared/Ui/Lists/MudPagedListComponentBase.cs"] =
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
