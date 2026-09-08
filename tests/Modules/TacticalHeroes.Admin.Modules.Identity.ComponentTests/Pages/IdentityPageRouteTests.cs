using Microsoft.AspNetCore.Components;

using CreateRolePageComponent = TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Ui.CreateRolePage;
using CreateUserPageComponent = TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Ui.CreateUserPage;
using UpdateRolePageComponent = TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Ui.UpdateRolePage;
using UpdateUserPageComponent = TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Ui.UpdateUserPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages;

public sealed class IdentityPageRouteTests
{
    [Fact(DisplayName = "RolePages should use separate components when routes are inspected")]
    public void RolePages_Should_UseSeparateComponents_When_RoutesAreInspected()
    {
        string[] createRoutes = GetRoutes<CreateRolePageComponent>();
        string[] updateRoutes = GetRoutes<UpdateRolePageComponent>();

        createRoutes.ShouldBe([IdentityRoutes.CreateRole]);
        updateRoutes.ShouldBe([IdentityRoutes.RoleTemplate]);
    }

    [Fact(DisplayName = "UserPages should use separate components when routes are inspected")]
    public void UserPages_Should_UseSeparateComponents_When_RoutesAreInspected()
    {
        string[] createRoutes = GetRoutes<CreateUserPageComponent>();
        string[] updateRoutes = GetRoutes<UpdateUserPageComponent>();

        createRoutes.ShouldBe([IdentityRoutes.CreateUser]);
        updateRoutes.ShouldBe([IdentityRoutes.UserTemplate]);
    }

    private static string[] GetRoutes<TComponent>()
    {
        return [.. typeof(TComponent)
            .GetCustomAttributes(typeof(RouteAttribute), inherit: false)
            .Cast<RouteAttribute>()
            .Select(attribute => attribute.Template)];
    }
}
