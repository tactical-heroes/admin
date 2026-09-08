using Microsoft.AspNetCore.Components;

using CreateFactionPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Ui.CreateFactionPage;
using UpdateFactionPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Ui.UpdateFactionPage;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages;

public sealed class FactionPageRouteTests
{
    [Fact(DisplayName = "Pages should use separate components when create and update routes are inspected")]
    public void Pages_Should_UseSeparateComponents_When_CreateAndUpdateRoutesAreInspected()
    {
        string[] createRoutes = GetRoutes<CreateFactionPageComponent>();
        string[] updateRoutes = GetRoutes<UpdateFactionPageComponent>();

        createRoutes.ShouldBe([CompendiumRoutes.CreateFaction]);
        updateRoutes.ShouldBe([CompendiumRoutes.FactionTemplate]);
    }

    private static string[] GetRoutes<TComponent>()
    {
        return [.. typeof(TComponent)
            .GetCustomAttributes(typeof(RouteAttribute), inherit: false)
            .Cast<RouteAttribute>()
            .Select(attribute => attribute.Template)];
    }
}
