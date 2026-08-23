using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Ui;

public partial class CreateRolePage(
    CreateRoleApi createRoleApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudCreateFormComponentBase<CreateRoleFormModel, CreateRoleFormModelValidator>(
        createRoleApi.CreateAsync,
        "Роль создана",
        IdentityRoutes.Role,
        snackbar,
        navigation)
{
}
