using Microsoft.AspNetCore.Components;

namespace Pl.Ui.Blazor.Admin.Components;

public partial class FiltersPanel
{
    [Parameter] public string Title { get; set; } = "Фильтры";
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public bool Expanded { get; set; }
    [Parameter] public EventCallback<bool> ExpandedChanged { get; set; }

    [Parameter] public EventCallback OnReset { get; set; }

    private Task OnResetClicked()
    {
        return OnReset.InvokeAsync();
    }
}
