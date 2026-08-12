using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Roles.EditRole;

public partial class RoleEditor
{
    private bool _loading;
    private bool _saving;
    private IReadOnlyDictionary<string, string[]> _fieldErrors =
        new Dictionary<string, string[]>();

    [Inject]
    private RolesApi RolesApi { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public Guid? Id { get; set; }

    [Parameter]
    public EventCallback Completed { get; set; }

    [PersistentState]
    public RoleDetails? Role { get; set; }

    [PersistentState]
    public string? LoadError { get; set; }

    private bool IsNew => !Id.HasValue;

    protected override async Task OnParametersSetAsync()
    {
        if (!Id.HasValue)
        {
            if (Role is null || Role.Id != Guid.Empty)
            {
                Role = new RoleDetails();
            }

            LoadError = null;
            _fieldErrors = new Dictionary<string, string[]>();
            return;
        }

        if (Role?.Id != Id.Value)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        if (!Id.HasValue)
        {
            return;
        }

        _loading = true;
        LoadError = null;
        _fieldErrors = new Dictionary<string, string[]>();

        Result<RoleDetails> result = await RolesApi.GetAsync(
            Id.Value,
            CancellationToken.None);

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

    private async Task SaveAsync()
    {
        if (Role is null)
        {
            return;
        }

        _saving = true;
        _fieldErrors = new Dictionary<string, string[]>();

        if (Role.Id == Guid.Empty)
        {
            Result<Guid> result = await RolesApi.CreateAsync(Role, CancellationToken.None);

            if (result.IsFailure)
            {
                HandleErrors(result.Errors);
                _saving = false;
                return;
            }

            Role.Id = result.Value;
            Snackbar.Add("Роль создана", Severity.Success);
        }
        else
        {
            Result result = await RolesApi.UpdateAsync(Role, CancellationToken.None);

            if (result.IsFailure)
            {
                HandleErrors(result.Errors);
                _saving = false;
                return;
            }

            Snackbar.Add("Роль сохранена", Severity.Success);
        }

        _saving = false;
        await Completed.InvokeAsync();
    }

    private void HandleErrors(IReadOnlyList<Error> errors)
    {
        _fieldErrors = ApiErrorMessage.GetFieldErrors(errors, MapField);
        IReadOnlyList<Error> unhandledErrors =
            ApiErrorMessage.GetUnhandledErrors(errors, MapField);

        if (unhandledErrors.Count > 0)
        {
            Snackbar.Add(ApiErrorMessage.FromErrors(unhandledErrors), Severity.Error);
        }
    }

    private static string? MapField(string field)
    {
        return string.Equals(field, nameof(RoleDetails.Name), StringComparison.OrdinalIgnoreCase)
            || string.Equals(field, "RoleName", StringComparison.OrdinalIgnoreCase)
                ? nameof(RoleDetails.Name)
                : null;
    }
}
