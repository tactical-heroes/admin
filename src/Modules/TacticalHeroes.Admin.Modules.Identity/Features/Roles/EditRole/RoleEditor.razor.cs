using Microsoft.AspNetCore.Components;
using MudBlazor;
using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Roles.EditRole;

public partial class RoleEditor
{
    private bool _loading;
    private bool _saving;

    [Inject]
    private RolesApi RolesApi { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    [Parameter]
    public EventCallback Saved { get; set; }

    [PersistentState]
    public RoleDetails? Role { get; set; }

    [PersistentState]
    public string? LoadError { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Role?.Id != Id && LoadError is null)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        LoadError = null;

        try
        {
            Role = await RolesApi.GetAsync(Id);
        }
        catch (Exception exception)
        {
            LoadError = ApiErrorMessage.FromException(exception);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task SaveAsync()
    {
        if (Role is null)
        {
            return;
        }

        _saving = true;

        try
        {
            await RolesApi.UpdateAsync(Role);
            Snackbar.Add("Роль сохранена", Severity.Success);
            await Saved.InvokeAsync();
        }
        catch (Exception exception)
        {
            Snackbar.Add(ApiErrorMessage.FromException(exception), Severity.Error);
        }
        finally
        {
            _saving = false;
        }
    }
}
