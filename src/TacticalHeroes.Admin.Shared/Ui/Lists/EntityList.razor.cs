using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace TacticalHeroes.Admin.Shared.Ui.Lists;

public partial class EntityList<TItem>
{
    private bool _filtersExpanded;
    private Dictionary<string, SortDefinition<TItem>> _sortDefinitions = [];
    private MudDataGrid<TItem>? _grid;
    private MudDataGrid<TItem>? _initializedGrid;
    private bool _restoringSorting;

    [Parameter]
    public string[] Sorting { get; set; } = [];

    [Parameter]
    public EventCallback<string[]> OnSortingChanged { get; set; }

    private Task ResetSortingAsync() => OnSortingChanged.InvokeAsync([]);

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
    public RenderFragment Columns { get; set; } = null!;

    protected override void OnParametersSet()
    {
        _sortDefinitions = new Dictionary<string, SortDefinition<TItem>>(StringComparer.OrdinalIgnoreCase);
        foreach (string criterion in Sorting)
        {
            string[] parts = criterion.Split(':', StringSplitOptions.TrimEntries);
            _sortDefinitions.TryAdd(parts[0], new SortDefinition<TItem>(
                parts[0],
                parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase),
                _sortDefinitions.Count,
                _ => null,
                null));
        }
    }

    private Task<GridData<TItem>> GetGridPageAsync(GridState<TItem> state, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GridData<TItem>
        {
            Items = Items ?? [],
            TotalItems = Items?.Count ?? 0
        });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_grid is null || ReferenceEquals(_grid, _initializedGrid))
        {
            return;
        }

        _initializedGrid = _grid;
        _restoringSorting = true;
        try
        {
            foreach (SortDefinition<TItem> definition in _sortDefinitions.Values.OrderBy(value => value.Index).ToArray())
            {
                await _grid.ExtendSortAsync(
                    definition.SortBy,
                    definition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                    definition.SortFunc);
            }
        }
        finally
        {
            _restoringSorting = false;
        }
    }

    private Task ChangeSortingAsync(Dictionary<string, SortDefinition<TItem>> definitions)
    {
        if (Loading || _restoringSorting)
        {
            return Task.CompletedTask;
        }

        string[] sorting = [.. definitions.Values
            .OrderBy(definition => definition.Index)
            .Select(definition => $"{definition.SortBy}:{(definition.Descending ? "desc" : "asc")}")];
        return OnSortingChanged.InvokeAsync(sorting);
    }

    private void ToggleFilters()
    {
        _filtersExpanded = !_filtersExpanded;
    }
}
