using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

using CreateHeroPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Ui.CreateHeroPage;
using HeroListPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.HeroListPage.Ui.HeroListPage;
using UpdateHeroPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Ui.UpdateHeroPage;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages;

public sealed class HeroPageRouteTests
{
    [Theory(DisplayName = "Pages should declare authorized routes when hero pages are inspected")]
    [InlineData(typeof(HeroListPageComponent), "/heroes")]
    [InlineData(typeof(CreateHeroPageComponent), "/heroes/new")]
    [InlineData(typeof(UpdateHeroPageComponent), "/heroes/{Id:guid}")]
    public void Pages_Should_DeclareAuthorizedRoutes_When_HeroPagesAreInspected(Type page, string route)
    {
        var routes = page.GetCustomAttributes(typeof(RouteAttribute), inherit: false).Cast<RouteAttribute>();
        object[] authorization = page.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false);

        routes.Select(attribute => attribute.Template).ShouldBe([route]);
        authorization.ShouldHaveSingleItem();
    }
}
