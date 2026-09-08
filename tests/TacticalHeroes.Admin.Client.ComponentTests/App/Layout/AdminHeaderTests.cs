using TacticalHeroes.Admin.Client.App.Layout;
using TacticalHeroes.Admin.Modules.Identity;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Layout;

public sealed class AdminHeaderTests : ClientComponentTestContext
{
    [Fact(DisplayName = "Render should invoke menu action when menu is clicked")]
    public void Render_Should_InvokeMenuAction_When_MenuIsClicked()
    {
        int clicks = 0;
        var component = Render<AdminHeader>(parameters => parameters.Add(header => header.OnMenuClick, () => clicks++));

        component.Find("button[aria-label='Открыть меню']").Click();

        clicks.ShouldBe(1);
    }

    [Theory(DisplayName = "Render should respect authentication when user state changes")]
    [InlineData(true)]
    [InlineData(false)]
    public void Render_Should_RespectAuthentication_When_UserStateChanges(bool authenticated)
    {
        if (authenticated)
        {
            AddAuthorization().SetAuthorized("Administrator");
        }

        var component = Render<AdminHeader>();

        component.FindAll(".current-user").Count.ShouldBe(authenticated ? 1 : 0);
        if (authenticated)
        {
            component.Find(".current-user").TextContent.ShouldBe("Administrator");
            component.Find("form").GetAttribute("action").ShouldBe(IdentityRoutes.AuthenticationLogout);
            component.Find("form").GetAttribute("method").ShouldBe("post");
        }
    }
}
