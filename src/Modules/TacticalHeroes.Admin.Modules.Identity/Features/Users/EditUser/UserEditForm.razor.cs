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

    [Parameter, EditorRequired]
    public IReadOnlyList<UserStatus> Statuses { get; set; } = [];

    [Parameter]
    public bool IsNew { get; set; }

    [Parameter]
    public bool Busy { get; set; }

    [Parameter]
    public EventCallback OnSave { get; set; }

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
