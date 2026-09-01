using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.UserListPage.Model;

public sealed class UserListFilterTests
{
    [Theory(DisplayName = "Compares filters by email")]
    [InlineData(null, null, true)]
    [InlineData("user@example.com", "user@example.com", true)]
    [InlineData("user@example.com", "other@example.com", false)]
    [InlineData("User@example.com", "user@example.com", false)]
    public void Equality_Should_CompareEmail(
        string? leftEmail,
        string? rightEmail,
        bool expected)
    {
        var left = new UserListFilter { Email = leftEmail };
        var right = new UserListFilter { Email = rightEmail };

        EqualityComparer<UserListFilter>.Default
            .Equals(left, right)
            .ShouldBe(expected);
    }
}
