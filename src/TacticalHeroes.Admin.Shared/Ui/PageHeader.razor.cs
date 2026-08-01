using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class PageHeader
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }
}
