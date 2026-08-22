using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Ui;

public partial class UpdateRolePage(
    UpdateRoleApi updateRoleApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudUpdateFormComponentBase<UpdateRoleFormModel, UpdateRoleFormModelValidator>(
        snackbar,
        navigation)
{
    protected override Task<Result<UpdateRoleFormModel>> LoadCoreAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return updateRoleApi.GetAsync(id, cancellationToken);
    }

    protected override Task<Result<Guid>> SaveCoreAsync()
    {
        return updateRoleApi.UpdateAsync(Id, Model, LifetimeToken);
    }

    protected override void OnSaveSucceeded(Guid _)
    {
        Snackbar.Add("Роль сохранена", Severity.Success);
        Navigation.NavigateTo(IdentityRoutes.Roles);
    }
}
