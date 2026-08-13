using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Ui;

public partial class CreateRolePage(
    CreateRoleApi createRoleApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudFormComponentBase<CreateRoleFormModel, CreateRoleFormModelValidator>(
        snackbar,
        navigation)
{
    protected override Task<Result<Guid>> SaveCoreAsync()
    {
        return createRoleApi.CreateAsync(Model, LifetimeToken);
    }

    protected override void OnSaveSucceeded(Guid id)
    {
        Snackbar.Add("Роль создана", Severity.Success);
        Navigation.NavigateTo(IdentityRoutes.Role(id));
    }
}
