using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui.Inputs;

public abstract partial class AsyncSelectOption<TId>(
    Func<string?, int, CancellationToken, Task<Result<IReadOnlyList<SelectOption<TId>>>>> loadAsync,
    string label,
    string emptyText,
    string? createHref = null,
    string createText = "Создать") : CancelableComponentBase
    where TId : notnull
{
    private long _loadVersion;

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
    public Expression<Func<TId>>? For { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public string? ErrorText { get; set; }

    [PersistentState(AllowUpdates = true)]
    public SelectOption<TId>? SelectedOption { get; set; }

    private IReadOnlyList<SelectOption<TId>> Options { get; set; } = [];

    private TId LoadedValue { get; set; } = default!;

    private string? LoadError { get; set; }

    private string? SearchError { get; set; }

    private bool IsLoading { get; set; }

    protected override async Task OnParametersSetAsync()
    {
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
            if (loadVersion != _loadVersion || !EqualityComparer<TId>.Default.Equals(Value, value))
            {
                return;
            }

            if (result.IsFailure)
            {
                LoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            Options = result.Value;
            SelectedOption = Options.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, value));
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
        if (search is null || search.Length < MinCharacters)
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
        return SelectedOption is not null && EqualityComparer<TId>.Default.Equals(SelectedOption.Id, id)
            ? SelectedOption.Name
            : Options.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, id))?.Name ?? string.Empty;
    }
}
