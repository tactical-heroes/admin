using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Ui;

public partial class CreateRolePage(CreateRoleApi createRoleApi)
{
    protected override Task<Result<Guid>> SaveCoreAsync()
    {
        return createRoleApi.CreateAsync(Model, LifetimeToken);
    }

    protected override void OnSaveSucceeded(Result<Guid> result)
    {
        Snackbar.Add("Роль создана", Severity.Success);
        Navigation.NavigateTo(IdentityRoutes.Role(result.Value));
    }
}
