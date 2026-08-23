using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Ui;

public partial class UpdateRolePage(
    UpdateRoleApi updateRoleApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudUpdateFormComponentBase<UpdateRoleFormModel, UpdateRoleFormModelValidator>(
        updateRoleApi.GetAsync,
        updateRoleApi.UpdateAsync,
        "Роль сохранена",
        IdentityRoutes.Roles,
        snackbar,
        navigation)
{
}
