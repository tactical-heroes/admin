using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Ui;

public partial class CreateRolePage
{
    private readonly CreateRoleFormModel Role = new();
    private readonly FormErrorState<CreateRoleFormModel> _errors = new();
    private readonly CreateRoleFormModelValidator _validator = new();
    private MudForm? _form;
    private bool _isValid;
    private bool _saving;

    [Inject]
    private CreateRoleApi CreateRoleApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private async Task SaveAsync()
    {
        _saving = true;
        _errors.Clear();

        Result<Guid> result = await CreateRoleApi.CreateAsync(Role, LifetimeToken);

        if (result.IsFailure)
        {
            _errors.Handle(result.Errors, Snackbar);
            _saving = false;
            return;
        }

        Snackbar.Add("Роль создана", Severity.Success);
        Navigation.NavigateTo(IdentityRoutes.Role(result.Value));
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
        {
            return;
        }

        await _form.ValidateAsync();

        if (_isValid)
        {
            await SaveAsync();
        }
    }
}
