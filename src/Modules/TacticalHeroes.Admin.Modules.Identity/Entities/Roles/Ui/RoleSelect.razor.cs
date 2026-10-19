using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Shared.Ui.Common;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Ui;

public partial class RoleSelect(RoleOptionsApi api) : CancelableComponentBase
{
    [Parameter, EditorRequired]
    public List<Guid> RoleIds { get; set; } = [];

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public string? ErrorText { get; set; }

    [PersistentState(AllowUpdates = true)]
    public List<RoleOption>? Items { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    private bool IsLoading { get; set; }

    protected override Task OnInitializedAsync()
    {
        return Items is null ? LoadAsync() : Task.CompletedTask;
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        LoadError = null;

        try
        {
            Result<IReadOnlyList<RoleOption>> result = await api.GetAllAsync(LifetimeToken);
            if (result.IsFailure)
            {
                LoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            Items = [.. result.Value];
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateRoles(IEnumerable<Guid>? values)
    {
        Guid[] selected = values?.Distinct().ToArray() ?? [];
        RoleIds.Clear();
        foreach (Guid id in selected)
        {
            RoleIds.Add(id);
        }
    }

    private string GetRoleName(Guid id)
    {
        RoleOption? role = Items?.FirstOrDefault(role => role.Id == id);
        return role is null
            ? $"Недоступная роль ({id})"
            : role.Name + (role.IsDefault ? " (по умолчанию)" : string.Empty);
    }
}
