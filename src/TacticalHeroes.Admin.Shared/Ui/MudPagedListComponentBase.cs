using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Model;
using TacticalHeroes.Admin.Shared.Navigation;

using EmptyPagedListFilter = System.ValueTuple;

namespace TacticalHeroes.Admin.Shared.Ui;

public abstract class MudPagedListComponentBase<TItem>(
    Func<int, int, CancellationToken, Task<Result<PaginationResult<TItem>>>> loadAsync,
    string listRoute,
    NavigationManager navigation)
    : MudPagedListComponentBase<TItem, EmptyPagedListFilter>(
        (pageNumber, pageSize, _, cancellationToken) =>
            loadAsync(pageNumber, pageSize, cancellationToken),
        listRoute,
        navigation)
{
    protected sealed override EmptyPagedListFilter AppliedFilter { get; } = new();
}

public abstract class MudPagedListComponentBase<
    TItem,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFilter>(
    Func<int, int, TFilter, CancellationToken, Task<Result<PaginationResult<TItem>>>> loadAsync,
    string listRoute,
    NavigationManager navigation)
    : CancelableComponentBase
    where TFilter : notnull, new()
{
    private static readonly EqualityComparer<TFilter> FilterComparer =
        EqualityComparer<TFilter>.Default;

    private long _loadVersion;

    [SupplyParameterFromQuery(Name = "page")]
    public int? PageNumber { get; set; }

    [SupplyParameterFromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    [PersistentState(AllowUpdates = true)]
    public PaginationResult<TItem>? Page { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [PersistentState(AllowUpdates = true)]
    public int? LoadedPageNumber { get; set; }

    [PersistentState(AllowUpdates = true)]
    public int? LoadedPageSize { get; set; }

    [PersistentState(AllowUpdates = true)]
    public TFilter? LoadedFilter { get; set; }

    protected bool IsLoading { get; private set; }

    protected TFilter FilterDraft { get; set; } = default!;

    protected int CurrentPageNumber => PageNumber is > 0
        ? PageNumber.Value
        : 1;

    protected int CurrentPageSize => PaginationOptions.NormalizePageSize(PageSize);

    protected int TotalPages => Page?.TotalPages ?? 0;

    protected long TotalCount => Page?.TotalCount ?? 0;

    protected bool HasActiveFilter => !FilterComparer.Equals(
        AppliedFilter,
        new TFilter());

    protected abstract TFilter AppliedFilter { get; }

    protected sealed override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        FilterDraft = AppliedFilter;

        if (!MatchesCurrentRoute())
        {
            await LoadPageAsync();
        }
    }

    protected async Task LoadPageAsync()
    {
        int pageNumber = CurrentPageNumber;
        int pageSize = CurrentPageSize;
        TFilter filter = AppliedFilter;
        long loadVersion = ++_loadVersion;

        IsLoading = true;
        LoadError = null;
        LoadedPageNumber = pageNumber;
        LoadedPageSize = pageSize;
        LoadedFilter = filter;

        try
        {
            Result<PaginationResult<TItem>> result = await loadAsync(
                pageNumber,
                pageSize,
                filter,
                LifetimeToken);

            if (!IsCurrentLoad(loadVersion, pageNumber, pageSize, filter))
            {
                return;
            }

            if (result.IsFailure)
            {
                Page = null;
                LoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            Page = result.Value;
        }
        finally
        {
            if (loadVersion == _loadVersion)
            {
                IsLoading = false;
            }
        }
    }

    protected void ChangePage(int pageNumber)
    {
        NavigateToList(AppliedFilter, pageNumber, CurrentPageSize);
    }

    protected void ChangePageSize(int pageSize)
    {
        NavigateToList(AppliedFilter, pageNumber: 1, pageSize);
    }

    protected void ApplyFilter()
    {
        ChangeFilter(FilterDraft);
    }

    protected void ResetFilter()
    {
        FilterDraft = new TFilter();
        ChangeFilter(FilterDraft);
    }

    protected void ChangeFilter(TFilter filter)
    {
        if (!FilterComparer.Equals(filter, AppliedFilter))
        {
            NavigateToList(filter, pageNumber: 1, CurrentPageSize);
        }
    }

    protected void NavigateToList(TFilter filter, int pageNumber, int pageSize)
    {
        navigation.NavigateTo(RouteUriBuilder.BuildPaged(
            listRoute,
            filter,
            pageNumber,
            pageSize));
    }

    protected async Task OnItemRemovedAsync()
    {
        if (Page?.Items.Count == 1 && CurrentPageNumber > 1)
        {
            ChangePage(CurrentPageNumber - 1);
        }
        else
        {
            await LoadPageAsync();
        }
    }

    private bool MatchesCurrentRoute()
    {
        return LoadedPageNumber == CurrentPageNumber
            && LoadedPageSize == CurrentPageSize
            && FilterComparer.Equals(LoadedFilter, AppliedFilter);
    }

    private bool IsCurrentLoad(
        long loadVersion,
        int pageNumber,
        int pageSize,
        TFilter filter)
    {
        return loadVersion == _loadVersion
            && pageNumber == CurrentPageNumber
            && pageSize == CurrentPageSize
            && FilterComparer.Equals(filter, AppliedFilter);
    }
}
