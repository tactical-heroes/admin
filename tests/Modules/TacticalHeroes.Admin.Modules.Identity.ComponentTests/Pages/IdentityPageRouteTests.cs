using Microsoft.AspNetCore.Components;

using CreateRolePageComponent = TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Ui.CreateRolePage;
using CreateUserPageComponent = TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Ui.CreateUserPage;
using UpdateRolePageComponent = TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Ui.UpdateRolePage;
using UpdateUserPageComponent = TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Ui.UpdateUserPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages;

public sealed class IdentityPageRouteTests
{
    [Fact(DisplayName = "Create and update roles use separate route components")]
    public void RolePages_Should_UseSeparateComponents()
    {
        GetRoutes<CreateRolePageComponent>().ShouldBe([IdentityRoutes.CreateRole]);
        GetRoutes<UpdateRolePageComponent>().ShouldBe([IdentityRoutes.RoleTemplate]);
    }

    [Fact(DisplayName = "Create and update users use separate route components")]
    public void UserPages_Should_UseSeparateComponents()
    {
        GetRoutes<CreateUserPageComponent>().ShouldBe([IdentityRoutes.CreateUser]);
        GetRoutes<UpdateUserPageComponent>().ShouldBe([IdentityRoutes.UserTemplate]);
    }

    private static string[] GetRoutes<TComponent>()
    {
        return typeof(TComponent)
            .GetCustomAttributes(typeof(RouteAttribute), inherit: false)
            .Cast<RouteAttribute>()
            .Select(attribute => attribute.Template)
            .ToArray();
    }
}
