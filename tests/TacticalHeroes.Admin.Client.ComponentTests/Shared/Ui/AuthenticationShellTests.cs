using TacticalHeroes.Admin.Client.Shared.Ui;

namespace TacticalHeroes.Admin.Client.ComponentTests.Shared.Ui;

public sealed class AuthenticationShellTests : BunitContext
{
    [Fact]
    public void Render_Should_MarkRequestedViewAsActive_When_ViewIsProvided()
    {
        var component = Render<AuthenticationShell>(parameters => parameters
            .Add(shell => shell.ActiveView, AuthenticationView.Confirmation)
            .AddChildContent("<p>Confirmation form</p>"));

        var activeLink = component.Find(".auth-navigation-link-active");

        activeLink.TextContent.Trim().ShouldBe("Подтверждение");
        activeLink.GetAttribute("aria-current").ShouldBe("page");
    }

    [Fact]
    public void Render_Should_PreserveReturnUrl_When_NavigatingBetweenViews()
    {
        const string returnUrl = "/connect/authorize?client_id=admin&scope=openid";

        var component = Render<AuthenticationShell>(parameters => parameters
            .Add(shell => shell.ActiveView, AuthenticationView.Login)
            .Add(shell => shell.ReturnUrl, returnUrl)
            .AddChildContent("<p>Login form</p>"));

        var links = component.FindAll(".auth-navigation-link");

        links[0].GetAttribute("href")
            .ShouldBe("/login?returnUrl=%2Fconnect%2Fauthorize%3Fclient_id%3Dadmin%26scope%3Dopenid");
        links[1].GetAttribute("href")
            .ShouldBe("/login?mode=register&returnUrl=%2Fconnect%2Fauthorize%3Fclient_id%3Dadmin%26scope%3Dopenid");
        links[2].GetAttribute("href")
            .ShouldBe("/login?mode=confirmation&returnUrl=%2Fconnect%2Fauthorize%3Fclient_id%3Dadmin%26scope%3Dopenid");
        links[3].GetAttribute("href")
            .ShouldBe("/login?mode=recover&returnUrl=%2Fconnect%2Fauthorize%3Fclient_id%3Dadmin%26scope%3Dopenid");
    }
}
