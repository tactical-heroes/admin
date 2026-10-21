using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace TacticalHeroes.Admin.Shared.Ui.Inputs;

public abstract partial class AsyncSelectOption<TId, TOption>(
    Func<string?, int, CancellationToken, Task<Result<IReadOnlyList<TOption>>>> loadAsync,
    string label,
    string emptyText,
    string? createHref = null,
    string createText = "Создать") : CancelableComponentBase
    where TId : notnull
    where TOption : SelectOption<TId>
{
    private MudAutocomplete<TId>? _autocomplete;
    private long _loadVersion;
    private readonly HashSet<TId> _loadedValues = [];

    protected AsyncSelectOption(
        Func<CancellationToken, Task<Result<IReadOnlyList<TOption>>>> loadAllAsync,
        string label,
        string emptyText) : this(CreateLocalSearch(loadAllAsync), label, emptyText)
    {
    }

    private string Label { get; } = label;

    private string EmptyText { get; } = emptyText;

    private string? CreateHref { get; } = createHref;

    private string CreateText { get; } = createText;

    [Parameter]
    public int MaxItems { get; set; } = 20;

    [Parameter]
    public int MinCharacters { get; set; } = 3;

    [Parameter]
    public int DebounceInterval { get; set; } = 300;

    [Parameter]
    public int MaxLength { get; set; } = 128;

    [Parameter]
    public TId Value { get; set; } = default!;

    [Parameter]
    public EventCallback<TId> ValueChanged { get; set; }

    [Parameter]
    public bool MultiSelection { get; set; }

    [Parameter]
    public List<TId> Values { get; set; } = [];

    [Parameter]
    public EventCallback<List<TId>> ValuesChanged { get; set; }

    [Parameter]
    public Expression<Func<TId>>? For { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public string? ErrorText { get; set; }

    [PersistentState(AllowUpdates = true)]
    public TOption? SelectedOption { get; set; }

    [PersistentState(AllowUpdates = true)]
    public List<TOption>? SelectedOptions { get; set; }

    private List<TOption> SelectedItems => SelectedOptions ??= [];

    private IReadOnlyList<TOption> Options { get; set; } = [];

    private TId LoadedValue { get; set; } = default!;

    private string? LoadError { get; set; }

    private string? SearchError { get; set; }

    private bool IsLoading { get; set; }

    private bool IsSearchTooShort { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (MultiSelection)
        {
            _loadedValues.IntersectWith(Values);
            SelectedItems.RemoveAll(option => !Values.Contains(option.Id));
            if (Values.Any(id => !_loadedValues.Contains(id) && SelectedItems.All(option => !EqualityComparer<TId>.Default.Equals(option.Id, id))))
            {
                await LoadAsync();
            }

            return;
        }

        if (!EqualityComparer<TId>.Default.Equals(Value, default) &&
            !EqualityComparer<TId>.Default.Equals(LoadedValue, Value) &&
            (SelectedOption is null || !EqualityComparer<TId>.Default.Equals(SelectedOption.Id, Value)))
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        var value = Value;
        long loadVersion = ++_loadVersion;
        LoadedValue = value;
        _loadedValues.UnionWith(Values);
        IsLoading = true;
        LoadError = null;

        try
        {
            var result = await loadAsync(null, MaxItems, LifetimeToken);
            if (loadVersion != _loadVersion || (!MultiSelection && !EqualityComparer<TId>.Default.Equals(Value, value)))
            {
                return;
            }

            if (result.IsFailure)
            {
                LoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            Options = result.Value;
            if (MultiSelection)
            {
                RememberSelectedOptions();
            }
            else
            {
                SelectedOption = Options.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, value));
            }
        }
        finally
        {
            if (loadVersion == _loadVersion)
            {
                IsLoading = false;
            }
        }
    }

    private async Task<IEnumerable<TId>> SearchAsync(string? search, CancellationToken cancellationToken)
    {
        search = search?.Trim();
        search = string.IsNullOrEmpty(search) ? null : search;
        IsSearchTooShort = search is not null && search.Length < MinCharacters;
        if (IsSearchTooShort)
        {
            Options = [];
            SearchError = null;
            await InvokeAsync(StateHasChanged);
            return [];
        }

        using var source = CancellationTokenSource.CreateLinkedTokenSource(LifetimeToken, cancellationToken);
        var result = await loadAsync(search, MaxItems, source.Token);
        source.Token.ThrowIfCancellationRequested();
        SearchError = result.IsFailure ? ApiErrorMessage.FromErrors(result.Errors) : null;
        Options = result.IsSuccess ? result.Value : [];
        if (MultiSelection)
        {
            RememberSelectedOptions();
        }

        await InvokeAsync(StateHasChanged);
        return Options.Select(option => option.Id).Where(id => !MultiSelection || !Values.Contains(id));
    }

    private async Task SetValueAsync(TId id)
    {
        if (MultiSelection)
        {
            var option = Options.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, id));
            if (option is null || Values.Contains(id))
            {
                return;
            }

            Values = [.. Values, id];
            RememberSelectedOptions();
            await ValuesChanged.InvokeAsync(Values);
            if (_autocomplete is not null)
            {
                await _autocomplete.ClearAsync();
            }

            return;
        }

        Value = id;
        SelectedOption = Options.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, id))
            ?? (SelectedOption is not null && EqualityComparer<TId>.Default.Equals(SelectedOption.Id, id) ? SelectedOption : null);
        await ValueChanged.InvokeAsync(id);
    }

    private string GetName(TId id)
    {
        var option = SelectedOption is not null && EqualityComparer<TId>.Default.Equals(SelectedOption.Id, id)
            ? SelectedOption
            : SelectedItems.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, id))
                ?? Options.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, id));
        return option is null ? string.Empty : GetOptionName(option);
    }

    protected virtual string GetOptionName(TOption option) => option.Name;

    private string GetSelectedName(TId id) => string.IsNullOrEmpty(GetName(id)) ? $"Недоступный вариант ({id})" : GetName(id);

    private void RememberSelectedOptions()
    {
        foreach (var option in Options.Where(option => Values.Contains(option.Id)))
        {
            SelectedItems.RemoveAll(selected => EqualityComparer<TId>.Default.Equals(selected.Id, option.Id));
            SelectedItems.Add(option);
        }
    }

    private async Task RemoveValueAsync(MudChip<TId> chip)
    {
        Values = Values.Where(id => !EqualityComparer<TId>.Default.Equals(id, chip.Value)).ToList();
        SelectedItems.RemoveAll(option => EqualityComparer<TId>.Default.Equals(option.Id, chip.Value));
        await ValuesChanged.InvokeAsync(Values);
    }

    private static Func<string?, int, CancellationToken, Task<Result<IReadOnlyList<TOption>>>> CreateLocalSearch(
        Func<CancellationToken, Task<Result<IReadOnlyList<TOption>>>> loadAllAsync)
    {
        IReadOnlyList<TOption>? catalog = null;
        return async (search, _, cancellationToken) =>
        {
            if (catalog is null)
            {
                var result = await loadAllAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (result.IsFailure)
                {
                    return result;
                }

                catalog = result.Value;
            }

            return Result.Success<IReadOnlyList<TOption>>(search is null
                ? catalog
                : catalog.Where(option => option.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToArray());
        };
    }
}
