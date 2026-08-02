using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class EntityRowActions
{
    [Parameter, EditorRequired]
    public string EditHref { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string EditLabel { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string DeleteLabel { get; set; } = string.Empty;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnDelete { get; set; }
}
