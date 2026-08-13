using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Ui;

public partial class UpdateRolePage
{
    private bool _loading;

    [Inject]
    private UpdateRoleApi UpdateRoleApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [PersistentState(AllowUpdates = true)]
    public Guid? LoadedId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (LoadedId != Id)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        LoadError = null;
        LoadedId = Id;
        Errors.Clear();

        Result<UpdateRoleFormModel> result = await UpdateRoleApi.GetAsync(
            Id,
            LifetimeToken);

        if (result.IsFailure)
        {
            LoadError = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            Model = result.Value;
        }

        _loading = false;
    }

    protected override Task ExecuteSaveAsync()
    {
        return SaveAsync(
            () => UpdateRoleApi.UpdateAsync(Id, Model, LifetimeToken),
            () =>
            {
                Snackbar.Add("Роль сохранена", Severity.Success);
                Navigation.NavigateTo(IdentityRoutes.Roles);
            });
    }
}
