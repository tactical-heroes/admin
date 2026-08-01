using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class EntityList<TItem>
{
    private bool _filtersExpanded;

    [Parameter]
    public IReadOnlyList<TItem>? Items { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public string? LoadError { get; set; }

    [Parameter, EditorRequired]
    public string EmptyText { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string RefreshLabel { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? Filters { get; set; }

    [Parameter]
    public bool HasFilters { get; set; }

    [Parameter]
    public bool HasActiveFilters { get; set; }

    [Parameter]
    public EventCallback OnApplyFilters { get; set; }

    [Parameter]
    public EventCallback OnResetFilters { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnRefresh { get; set; }

    [Parameter]
    public int PageNumber { get; set; } = 1;

    [Parameter]
    public int PageSize { get; set; } = 10;

    [Parameter]
    public int TotalPages { get; set; }

    [Parameter]
    public long TotalCount { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<int> OnPageNumberChanged { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<int> OnPageSizeChanged { get; set; }

    [Parameter, EditorRequired]
    public RenderFragment HeaderContent { get; set; } = null!;

    [Parameter, EditorRequired]
    public RenderFragment<TItem> RowTemplate { get; set; } = null!;

    private void ToggleFilters()
    {
        _filtersExpanded = !_filtersExpanded;
    }
}
