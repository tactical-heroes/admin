namespace TacticalHeroes.Admin.Shared.ComponentTests.Navigation;

public sealed class RouteUriBuilderTests
{
    [Fact(DisplayName = "Build should encode query properties when values are provided")]
    public void Build_Should_EncodeQueryProperties_When_ValuesAreProvided()
    {
        string route = RouteUriBuilder.Build(
            "/confirm-email",
            new
            {
                UserId = Guid.Parse("bc49d005-4cbc-4941-985d-1354cb6c68d3"),
                Token = "token/+==",
            });

        route.ShouldBe(
            "/confirm-email?userId=bc49d005-4cbc-4941-985d-1354cb6c68d3" +
            "&token=token%2F%2B%3D%3D");
    }

    [Fact(DisplayName = "BuildPaged should encode filter and pagination when values are provided")]
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

    [Fact(DisplayName = "BuildPaged should omit query when values are empty or default")]
    public void BuildPaged_Should_OmitQuery_When_ValuesAreEmptyOrDefault()
    {
        string route = RouteUriBuilder.BuildPaged(
            "/users",
            new TestFilter(),
            pageNumber: 1,
            pageSize: 10);

        route.ShouldBe("/users");
    }

    [Fact(DisplayName = "BuildPaged should preserve readable sorting and encoded values when special characters are provided")]
    public void BuildPaged_Should_PreserveReadableSortingAndEncodedValues_When_SpecialCharactersAreProvided()
    {
        const string email = "name:tag+test&x=#%3A@example.test";
        string[] sorting = ["IsConfirmed:asc", "Email:desc", "Name&x=#%3A:asc"];

        string route = RouteUriBuilder.BuildPaged(
            "/users", new TestFilter { Email = email }, 1, 10, sorting);
        var query = System.Web.HttpUtility.ParseQueryString(new Uri("https://example.test" + route).Query);

        route.ShouldBe("/users?email=name%3Atag%2Btest%26x%3D%23%253A%40example.test" +
            "&sort=IsConfirmed:asc&sort=Email:desc&sort=Name%26x%3D%23%253A:asc");
        query["email"].ShouldBe(email);
        query.GetValues("sort").ShouldBe(sorting);
        query.AllKeys.ShouldBe(["email", "sort"]);
    }

    private sealed record TestFilter
    {
        public string? Email { get; set; }

        public int? MinimumAge { get; set; }

        public string[] Roles { get; set; } = [];
    }
}
