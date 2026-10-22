using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui.Inputs;

public abstract partial class AsyncMultiSelectOption<TId, TOption>(
    Func<CancellationToken, Task<Result<IReadOnlyList<TOption>>>> loadAllAsync,
    string label,
    string emptyText,
    string? helperText = null) : CancelableComponentBase
    where TId : notnull
    where TOption : SelectOption<TId>
{
    private string Label { get; } = label;

    private string EmptyText { get; } = emptyText;

    [Parameter]
    public List<TId> Values { get; set; } = [];

    [Parameter]
    public EventCallback<List<TId>> ValuesChanged { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public string? ErrorText { get; set; }

    [Parameter]
    public string? HelperText { get; set; } = helperText;

    [PersistentState(AllowUpdates = true)]
    public List<TOption>? Items { get; set; }

    private string? LoadError { get; set; }

    private bool IsLoading { get; set; }

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
        if (Items is null && !IsLoading && LoadError is null)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        LoadError = null;

        try
        {
            var result = await loadAllAsync(LifetimeToken);
            LifetimeToken.ThrowIfCancellationRequested();
            if (result.IsFailure)
            {
                LoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            Items = [.. result.Value];
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual string GetOptionName(TOption option) => option.Name;

    private string GetSelectedName(TId id)
    {
        var option = Items?.FirstOrDefault(option => EqualityComparer<TId>.Default.Equals(option.Id, id));
        return option is null ? $"Недоступный вариант ({id})" : GetOptionName(option);
    }

    private async Task SetValuesAsync(IEnumerable<TId> values)
    {
        Values = [.. values.Distinct()];
        await ValuesChanged.InvokeAsync(Values);
    }
}
