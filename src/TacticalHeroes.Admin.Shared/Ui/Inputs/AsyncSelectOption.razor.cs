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

    protected AsyncSelectOption(
        Func<CancellationToken, Task<Result<IReadOnlyList<TOption>>>> loadAllAsync,
        string label,
        string emptyText,
        string? helperText = null) : this(CreateLocalSearch(loadAllAsync), label, emptyText)
    {
        HelperText = helperText;
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

    [Parameter]
    public string? HelperText { get; set; }

    [PersistentState(AllowUpdates = true)]
    public TOption? SelectedOption { get; set; }

    [PersistentState(AllowUpdates = true)]
    public List<TOption>? Items { get; set; }

    private IReadOnlyList<TOption> Options { get; set; } = [];

    private TId LoadedValue { get; set; } = default!;

    private string? LoadError { get; set; }

    private string? SearchError { get; set; }

    private bool IsLoading { get; set; }

    private bool IsSearchTooShort { get; set; }

    private string MultiSelectDescriptionId { get; } = $"select-description-{Guid.NewGuid():N}";

    private bool IsMultiSelectOpen { get; set; }

    private bool ShowAllSelected { get; set; }

    private List<TId> SelectedIds => [.. Values.Distinct()];

    private List<TId> OptionIds => [.. (Items ?? []).Select(option => option.Id).Concat(Values).Distinct()];

    private bool IsMissing(TId id) => Items?.All(option => !EqualityComparer<TId>.Default.Equals(option.Id, id)) != false;

    private void ToggleSelected() => ShowAllSelected = !ShowAllSelected;

    private Task RemoveValueAsync(TId id) => SetValuesAsync(Values.Where(value => !EqualityComparer<TId>.Default.Equals(value, id)));

    private Task ToggleValueAsync(TId id) => Values.Contains(id) ? RemoveValueAsync(id) : SetValuesAsync(Values.Append(id));

    protected override async Task OnParametersSetAsync()
    {
        if (MultiSelection)
        {
            if (Items is null && !IsLoading && LoadError is null)
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

            if (MultiSelection)
            {
                Items = [.. result.Value];
            }
            else
            {
                Options = result.Value;
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
        await InvokeAsync(StateHasChanged);
        return Options.Select(option => option.Id);
    }

    private async Task SetValueAsync(TId id)
    {
        Value = id;
        SelectedOption = Options.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, id))
            ?? (SelectedOption is not null && EqualityComparer<TId>.Default.Equals(SelectedOption.Id, id) ? SelectedOption : null);
        await ValueChanged.InvokeAsync(id);
    }

    private string GetName(TId id)
    {
        var option = SelectedOption is not null && EqualityComparer<TId>.Default.Equals(SelectedOption.Id, id)
            ? SelectedOption
            : Options.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, id));
        return option is null ? string.Empty : GetOptionName(option);
    }

    protected virtual string GetOptionName(TOption option) => option.Name;

    private string GetSelectedName(TId id)
    {
        var option = Items?.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, id));
        return option is null ? $"Недоступный вариант ({id})" : GetOptionName(option);
    }

    private async Task SetValuesAsync(IEnumerable<TId>? values)
    {
        Values = values is null ? [] : [.. values.Distinct()];
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
                : [.. catalog.Where(option => option.Name.Contains(search, StringComparison.OrdinalIgnoreCase))]);
        };
    }
}
