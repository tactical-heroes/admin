using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui.Lists;

public partial class EntityLink
{
    [Parameter, EditorRequired]
    public string Href { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string Text { get; set; } = string.Empty;
}
