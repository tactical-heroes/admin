using Microsoft.AspNetCore.Components;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Ui;

public partial class RoleListPage(
    RoleListApi roleListApi,
    NavigationManager navigation)
    : MudPagedListComponentBase<RoleListItem>(
        roleListApi.GetPageAsync,
        IdentityRoutes.RolesPage,
        navigation)
{
    private Task<Result> DeleteRoleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return roleListApi.DeleteAsync(id, cancellationToken);
    }
}
