using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using TacticalHeroes.Admin.Shared.Authorization;

namespace TacticalHeroes.Admin.Shared.Ui.Common;

public partial class PermissionView
{
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    [Parameter, EditorRequired]
    public IReadOnlyCollection<string> Permissions { get; set; } = [];

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool _allowed;

    protected override async Task OnParametersSetAsync()
    {
        _allowed = false;
        if (AuthenticationState is not null && Permissions.Count > 0)
        {
            var state = await AuthenticationState;
            _allowed = Permissions.All(permission => PermissionAccess.HasPermission(state.User, permission));
        }
    }
}
