using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Client.Pages.NotFound;

namespace TacticalHeroes.Admin.Client.ComponentTests.Pages.NotFound;

public sealed class NotFoundPageTests : ClientComponentTestContext
{
    [Fact(DisplayName = "Render should offer home link when page is not found")]
    public void Render_Should_OfferHomeLink_When_PageIsNotFound()
    {
        var component = Render<NotFoundPage>();

        component.Find("h1").TextContent.Trim().ShouldBe("Страница не найдена");
        component.Find("a").GetAttribute("href").ShouldBe(AdminRoutes.Home);
    }
}
