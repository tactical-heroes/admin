using Microsoft.AspNetCore.Components;
using MudBlazor;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Roles.EditRole;

public partial class RoleEditForm
{
    private MudForm? _form;
    private bool _isValid;

    [Parameter, EditorRequired]
    public RoleDetails Model { get; set; } = new();

    [Parameter]
    public bool IsNew { get; set; }

    [Parameter]
    public bool Busy { get; set; }

    [Parameter]
    public EventCallback OnSave { get; set; }

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
