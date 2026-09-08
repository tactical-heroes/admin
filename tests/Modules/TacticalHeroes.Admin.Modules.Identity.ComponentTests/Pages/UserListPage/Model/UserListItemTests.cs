using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.UserListPage.Model;

public sealed class UserListItemTests
{
    [Theory(DisplayName = "StatusColor should return expected color when status is mapped")]
    [InlineData("Active", Color.Success)]
    [InlineData("Blocked", Color.Error)]
    [InlineData("Unknown", Color.Default)]
    public void StatusColor_Should_ReturnExpectedColor_When_StatusIsMapped(
        string status,
        Color expectedColor)
    {
        var user = new UserListItem
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            UserName = "user",
            IsConfirmed = true,
            Status = status,
            StatusDisplayName = status,
        };

        user.StatusColor.ShouldBe(expectedColor);
    }
}
