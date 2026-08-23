using TacticalHeroes.Admin.Shared.Navigation;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Navigation;

public sealed class RouteUriBuilderTests
{
    [Fact(DisplayName = "Builds paged route from filter properties")]
    public void BuildPaged_Should_EncodeFilterAndPagination_When_ValuesAreProvided()
    {
        var filter = new TestFilter
        {
            Email = "hero@example.com",
            MinimumAge = 18,
            Roles = ["admin", "moderator"],
        };

        string route = RouteUriBuilder.BuildPaged(
            "/users",
            filter,
            pageNumber: 3,
            pageSize: 25);

        route.ShouldBe(
            "/users?email=hero%40example.com&minimumAge=18" +
            "&roles=admin&roles=moderator&page=3&pageSize=25");
    }

    [Fact(DisplayName = "Omits empty filter and default pagination")]
    public void BuildPaged_Should_OmitQuery_When_ValuesAreEmptyOrDefault()
    {
        string route = RouteUriBuilder.BuildPaged(
            "/users",
            new TestFilter(),
            pageNumber: 1,
            pageSize: 10);

        route.ShouldBe("/users");
    }

    private sealed record TestFilter
    {
        public string? Email { get; set; }

        public int? MinimumAge { get; set; }

        public string[] Roles { get; set; } = [];
    }
}
