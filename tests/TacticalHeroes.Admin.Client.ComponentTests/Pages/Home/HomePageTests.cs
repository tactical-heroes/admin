using TacticalHeroes.Admin.Client.Pages.Home;

namespace TacticalHeroes.Admin.Client.ComponentTests.Pages.Home;

public sealed class HomePageTests : ClientComponentTestContext
{
    [Fact(DisplayName = "The home page renders its heading and welcome content")]
    public void Render_Should_ShowWelcome_When_PageIsRendered()
    {
        var component = Render<HomePage>();

        component.Find("h1").TextContent.ShouldBe("Главная");
        component.Find("#welcome-title").TextContent.Trim().ShouldBe("Добро пожаловать");
    }
}
