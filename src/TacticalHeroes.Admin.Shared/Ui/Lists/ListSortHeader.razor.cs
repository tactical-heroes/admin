using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace TacticalHeroes.Admin.Shared.Ui.Lists;

public partial class ListSortHeader
{
    [Parameter, EditorRequired]
    public string Field { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public string[] Sorting { get; set; } = [];

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<string[]> SortingChanged { get; set; }

    private int Index => Array.FindIndex(Sorting, value => string.Equals(
        value.Split(':', 2)[0].Trim(), Field, StringComparison.OrdinalIgnoreCase));

    private bool Descending => Index >= 0 && string.Equals(
        Sorting[Index].Split(':', 2, StringSplitOptions.TrimEntries).ElementAtOrDefault(1),
        "desc",
        StringComparison.OrdinalIgnoreCase);

    private string AriaSort => Index < 0 ? "none" : Descending ? "descending" : "ascending";

    private string SortIcon => Index < 0
        ? Icons.Material.Filled.UnfoldMore
        : Descending ? Icons.Material.Filled.ArrowDownward : Icons.Material.Filled.ArrowUpward;

    private Task ToggleAsync()
    {
        List<string> sorting = [.. Sorting];
        int index = Index;
        if (index < 0)
        {
            sorting.Add($"{Field}:asc");
        }
        else if (Descending)
        {
            sorting.RemoveAt(index);
        }
        else
        {
            sorting[index] = $"{Field}:desc";
        }

        return SortingChanged.InvokeAsync([.. sorting]);
    }
}
