using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.UserListPage.Model;

public sealed class UserListItemTests
{
    [Theory(DisplayName = "Maps user statuses to their presentation colors")]
    [InlineData("Active", Color.Success)]
    [InlineData("Blocked", Color.Error)]
    [InlineData("Unknown", Color.Default)]
    public void StatusColor_Should_ReturnExpectedColor(
        string status,
        Color expectedColor)
    {
        var user = new UserListItem(
            Guid.NewGuid(),
            "user@example.com",
            "user",
            true,
            status,
            status);

        user.StatusColor.ShouldBe(expectedColor);
    }
}
