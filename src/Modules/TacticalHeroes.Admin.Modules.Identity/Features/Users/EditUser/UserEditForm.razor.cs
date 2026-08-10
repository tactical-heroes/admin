using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Users.EditUser;

public partial class UserEditForm
{
    private MudForm? _form;
    private bool _isValid;

    [Parameter, EditorRequired]
    public UserDetails Model { get; set; } = new();

    [Parameter]
    public IReadOnlyDictionary<string, string[]> Errors { get; set; } =
        new Dictionary<string, string[]>();

    [Parameter, EditorRequired]
    public IReadOnlyList<UserStatus> Statuses { get; set; } = [];

    [Parameter]
    public bool IsNew { get; set; }

    [Parameter]
    public bool Busy { get; set; }

    [Parameter]
    public EventCallback OnSave { get; set; }

    private bool HasError(string field)
    {
        return Errors.ContainsKey(field);
    }

    private string? GetError(string field)
    {
        return Errors.TryGetValue(field, out string[]? messages)
            ? string.Join(" ", messages)
            : null;
    }

    private string GetStatusDisplayName(string? statusName)
    {
        return Statuses.FirstOrDefault(status => status.Name == statusName)?.DisplayName
            ?? statusName
            ?? string.Empty;
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
            await OnSave.InvokeAsync();
        }
    }
}
