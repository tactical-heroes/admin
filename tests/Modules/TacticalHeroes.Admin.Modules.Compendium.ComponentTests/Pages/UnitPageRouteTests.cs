using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

using CreateUnitPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Ui.CreateUnitPage;
using UnitListPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Ui.UnitListPage;
using UpdateUnitPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Ui.UpdateUnitPage;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages;

public sealed class UnitPageRouteTests
{
    [Theory(DisplayName = "Pages should declare authorized routes when unit pages are inspected")]
    [InlineData(typeof(UnitListPageComponent), "/units")]
    [InlineData(typeof(CreateUnitPageComponent), "/units/new")]
    [InlineData(typeof(UpdateUnitPageComponent), "/units/{Id:guid}")]
    public void Pages_Should_DeclareAuthorizedRoutes_When_UnitPagesAreInspected(Type page, string route)
    {
        var routes = page.GetCustomAttributes(typeof(RouteAttribute), inherit: false).Cast<RouteAttribute>();
        object[] authorization = page.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false);

        routes.Select(attribute => attribute.Template).ShouldBe([route]);
        authorization.ShouldHaveSingleItem();
    }
}
