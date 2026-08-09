using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class ListFilters
{
    [Parameter]
    public bool Expanded { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public bool HasFilters { get; set; }

    [Parameter]
    public bool HasActiveFilters { get; set; }

    [Parameter]
    public EventCallback OnApply { get; set; }

    [Parameter]
    public EventCallback OnReset { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
