using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Ui;

public partial class CreateRolePage
{
    [Inject]
    private CreateRoleApi CreateRoleApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    protected override Task ExecuteSaveAsync()
    {
        return SaveAsync(
            () => CreateRoleApi.CreateAsync(Model, LifetimeToken),
            id =>
            {
                Snackbar.Add("Роль создана", Severity.Success);
                Navigation.NavigateTo(IdentityRoutes.Role(id));
            });
    }
}
