using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui.Lists;

public partial class EntityRowActions
{
    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;
}
