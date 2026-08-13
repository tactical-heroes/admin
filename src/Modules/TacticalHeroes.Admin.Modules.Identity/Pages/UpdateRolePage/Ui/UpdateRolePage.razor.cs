using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Ui;

public partial class UpdateRolePage
{
    private readonly FormErrorState<UpdateRoleFormModel> _errors = new();
    private readonly UpdateRoleFormModelValidator _validator = new();
    private bool _loading;

    [Inject]
    private UpdateRoleApi UpdateRoleApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    [PersistentState(AllowUpdates = true)]
    public UpdateRoleFormModel? Role { get; set; }

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
        Role = null;
        LoadError = null;
        LoadedId = Id;
        _errors.Clear();

        Result<UpdateRoleFormModel> result = await UpdateRoleApi.GetAsync(
            Id,
            LifetimeToken);

        if (result.IsFailure)
        {
            LoadError = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            Role = result.Value;
        }

        _loading = false;
    }

    protected override async Task SaveAsync()
    {
        if (Role is null)
        {
            return;
        }

        _errors.Clear();

        Result result = await UpdateRoleApi.UpdateAsync(Id, Role, LifetimeToken);

        if (result.IsFailure)
        {
            _errors.Handle(result.Errors, Snackbar);
            return;
        }

        Snackbar.Add("Роль сохранена", Severity.Success);
        Navigation.NavigateTo(IdentityRoutes.Roles);
    }
}
