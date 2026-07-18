using Microsoft.AspNetCore.Components;

namespace Pl.Ui.Blazor.Admin.Components;

public partial class ActionsPanel
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
