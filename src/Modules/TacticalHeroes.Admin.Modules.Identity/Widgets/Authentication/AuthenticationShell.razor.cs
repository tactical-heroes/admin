using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Identity.Widgets.Authentication;

public partial class AuthenticationShell
{
    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;
}
