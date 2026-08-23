using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class EntityRowActions
{
    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;
}
