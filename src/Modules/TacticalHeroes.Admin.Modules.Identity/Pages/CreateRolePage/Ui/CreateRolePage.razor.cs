using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Ui;

public partial class CreateRolePage
{
    [Inject]
    private CreateRoleApi CreateRoleApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    protected override async Task SaveAsync()
    {
        Errors.Clear();

        Result<Guid> result = await CreateRoleApi.CreateAsync(Model, LifetimeToken);

        if (result.IsFailure)
        {
            Errors.Handle(result.Errors, Snackbar);
            return;
        }

        Snackbar.Add("Роль создана", Severity.Success);
        Navigation.NavigateTo(IdentityRoutes.Role(result.Value));
    }
}
