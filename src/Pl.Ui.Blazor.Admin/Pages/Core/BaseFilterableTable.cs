using Common.Enums;
using Common.SearchParams.Core;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Pl.Ui.Blazor.Services.Interfaces.Core;

namespace Pl.Ui.Blazor.Admin.Pages.Core;

public abstract class BaseFilterableTable<TId, TViewModel, TSearchParams, TConvertParams, TService> : ComponentBase
    where TId : struct
    where TViewModel : class, new()
    where TSearchParams : BaseSearchParams, new()
    where TConvertParams : class, new()
    where TService : IBaseService<TId, TViewModel, TSearchParams, TConvertParams>
{
    [Inject] protected TService Service { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected abstract string EditRoute { get; }

    protected const string AddButtonText = "Добавить";
    protected const string Actions = "Действия";

    protected MudTable<TViewModel>? _table;

    protected bool _loading;
    protected bool _filtersExpanded;
    protected int _currentPage;

    protected TSearchParams _searchParams = new();
    protected TConvertParams _convertParams = new();

    protected virtual Task OnFiltersExpandedChanged(bool expanded)
    {
        _filtersExpanded = expanded;
        return Task.CompletedTask;
    }

    protected virtual async Task OnFiltersChanged()
    {
        if (_table is null)
        {
            return;
        }

        if (_currentPage != 0)
        {
            _table.NavigateTo(Page.First);
            return;
        }

        await _table.ReloadServerData();
    }

    protected virtual async Task ResetFilters()
    {
        _searchParams = new TSearchParams();
        await OnFiltersChanged();
    }

    protected virtual async Task<TableData<TViewModel>> LoadServerData(TableState state, CancellationToken cancellationToken)
    {
        _loading = true;

        try
        {
            _currentPage = state.Page;
            ApplyTableState(_searchParams, state);

            var result = await Service.GetAsync(_searchParams, _convertParams, cancellationToken);

            return new TableData<TViewModel>
            {
                TotalItems = result.Total > int.MaxValue ? int.MaxValue : (int)result.Total,
                Items = result.Objects
            };
        }
        finally
        {
            _loading = false;
        }
    }

    protected void OnAddClicked()
    {
        Navigation.NavigateTo(EditRoute);
    }

    protected void OnEditClicked(Guid id)
    {
        Navigation.NavigateTo($"{EditRoute}/{id}");
    }

    protected async Task OnDeleteClicked(TId id)
    {
        var confirmed = await DialogService.ShowMessageBox(
            title: "Удаление",
            message: "Вы уверены, что хотите выполнить это действие?",
            yesText: "Удалить",
            cancelText: "Отмена");

        if (confirmed != true)
        {
            return;
        }

        try
        {
            _loading = true;

            await Service.DeleteAsync(id);

            Snackbar.Add("Успешно.", Severity.Success);

            if (_table is not null)
            {
                await _table.ReloadServerData();
            }
        }
        catch (Exception)
        {
            Snackbar.Add($"Ошибка.", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    protected virtual int[] GetPageSizeOptions()
    {
        return [10, 20, 50];
    }

    private static void ApplyTableState(BaseSearchParams searchParams, TableState state, IReadOnlyDictionary<string, string>? sortFieldMap = null)
    {
        ArgumentNullException.ThrowIfNull(searchParams);
        ArgumentNullException.ThrowIfNull(state);

        searchParams.Page = state.Page + 1;
        searchParams.ObjectsCount = state.PageSize;

        ApplySorting(searchParams, state, sortFieldMap);
    }

    private static void ApplySorting(BaseSearchParams searchParams, TableState state, IReadOnlyDictionary<string, string>? sortFieldMap)
    {
        if (state.SortDirection == SortDirection.None || string.IsNullOrWhiteSpace(state.SortLabel))
        {
            searchParams.SortField = null;
            searchParams.SortOrder = default;
            return;
        }

        var sortField = state.SortLabel;

        if (sortFieldMap is not null && sortFieldMap.TryGetValue(sortField, out var mapped))
        {
            sortField = mapped;
        }

        searchParams.SortField = sortField;
        searchParams.SortOrder = MapSortOrder(state.SortDirection);
    }

    private static SortOrder MapSortOrder(SortDirection direction)
    {
        if (direction == SortDirection.Descending)
        {
            return SortOrder.Descending;
        }

        if (direction == SortDirection.Ascending)
        {
            return SortOrder.Ascending;
        }

        return default;
    }
}
