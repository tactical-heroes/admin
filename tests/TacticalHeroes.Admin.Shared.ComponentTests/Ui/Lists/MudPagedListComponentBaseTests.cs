using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Lists;

public sealed class MudPagedListComponentBaseTests : BunitContext
{
    [Fact(DisplayName = "ChangePage should preserve filter when page changes")]
    public async Task ChangePage_Should_PreserveFilter_When_PageChanges()
    {
        TestComponent component = CreateComponent();
        await component.SetRouteAsync(2, 25, new TestFilter { Email = "admin@example.test" });

        component.ChangePage(3);

        component.CurrentUri.ShouldEndWith("/items?email=admin%40example.test&page=3&pageSize=25");
    }

    [Fact(DisplayName = "ChangePageSize should reset page when page size changes")]
    public async Task ChangePageSize_Should_ResetPage_When_PageSizeChanges()
    {
        TestComponent component = CreateComponent();
        await component.SetRouteAsync(2, 25, new TestFilter { Email = "admin@example.test" });

        component.ChangePageSize(50);

        component.CurrentUri.ShouldEndWith("/items?email=admin%40example.test&pageSize=50");
    }

    [Fact(DisplayName = "OnItemRemovedAsync should navigate back when last item is removed")]
    public async Task OnItemRemovedAsync_Should_NavigateBack_When_LastItemIsRemoved()
    {
        TestComponent component = CreateComponent();
        await component.SetRouteAsync(2, 25);

        await component.NotifyItemRemovedAsync();

        component.CurrentUri.ShouldEndWith("/items?pageSize=25");
    }

    [Fact(DisplayName = "OnParametersSetAsync should load once when route state is unchanged")]
    public async Task OnParametersSetAsync_Should_LoadOnce_When_RouteStateIsUnchanged()
    {
        TestComponent component = CreateComponent();

        var filter = new TestFilter
        {
            Email = "admin@example.test",
            MinimumAge = 18,
        };

        await component.SetRouteAsync(2, 25, filter);
        await component.SetRouteAsync(2, 25, filter);
        await component.SetRouteAsync(3, 25, filter);

        component.LoadRequests
            .Select(request => (
                request.PageNumber,
                request.PageSize,
                request.Filter.Email,
                request.Filter.MinimumAge))
            .ShouldBe(
        [
            (2, 25, "admin@example.test", 18),
            (3, 25, "admin@example.test", 18),
        ]);
        component.Page?.PageNumber.ShouldBe(3);
    }

    [Fact(DisplayName = "Totals should be zero when page is unavailable")]
    public void Totals_Should_BeZero_When_PageIsUnavailable()
    {
        TestComponent component = CreateComponent();

        component.Pages.ShouldBe(0);
        component.ItemsCount.ShouldBe(0);
    }

    [Fact(DisplayName = "ApplyFilter should navigate from first page when draft changes")]
    public async Task ApplyFilter_Should_NavigateFromFirstPage_When_DraftChanges()
    {
        TestComponent component = CreateComponent();

        await component.SetRouteAsync(2, 25, new TestFilter
        {
            Email = "admin@example.test",
            MinimumAge = 18,
        });

        component.DraftFilter.Email.ShouldBe("admin@example.test");
        component.DraftFilter.MinimumAge.ShouldBe(18);
        component.FilterIsActive.ShouldBeTrue();

        component.DraftFilter.Email = "moderator@example.test";
        component.DraftFilter.MinimumAge = 21;
        component.ApplyDraftFilter();

        component.CurrentUri.ShouldEndWith(
            "/items?email=moderator%40example.test&minimumAge=21&pageSize=25");
    }

    [Fact(DisplayName = "ResetFilter should clear draft and navigate when filter is active")]
    public async Task ResetFilter_Should_ClearDraftAndNavigate_When_FilterIsActive()
    {
        TestComponent component = CreateComponent();
        await component.SetRouteAsync(2, 25, new TestFilter
        {
            Email = "admin@example.test",
            MinimumAge = 18,
        });

        component.ResetDraftFilter();

        component.DraftFilter.Email.ShouldBeNull();
        component.DraftFilter.MinimumAge.ShouldBeNull();
        component.CurrentUri.ShouldEndWith(
            "/items?pageSize=25");
    }

    [Fact(DisplayName = "ChangeFilter should keep current uri when filter is unchanged")]
    public async Task ChangeFilter_Should_KeepCurrentUri_When_FilterIsUnchanged()
    {
        TestComponent component = CreateComponent();
        await component.SetRouteAsync(2, 25, new TestFilter { Email = "admin@example.test" });
        string originalUri = component.CurrentUri;

        component.ApplyDraftFilter();

        component.CurrentUri.ShouldBe(originalUri);
    }

    [Fact(DisplayName = "NavigateToList should include filter and pagination when navigating")]
    public void NavigateToList_Should_IncludeFilterAndPagination_When_Navigating()
    {
        TestComponent component = CreateComponent();

        component.NavigateToList(new TestFilter { Email = "admin+test@example.test" }, 3, 25);

        component.CurrentUri.ShouldEndWith("/items?email=admin%2Btest%40example.test&page=3&pageSize=25");
    }

    [Fact(DisplayName = "MatchesCurrentRoute should reuse page when persisted state matches")]
    public async Task MatchesCurrentRoute_Should_ReusePage_When_PersistedStateMatches()
    {
        TestComponent component = CreateComponent();
        PaginationResult<TestItem> page = CreatePage(2, 25);
        component.Page = page;
        component.LoadedPageNumber = 2;
        component.LoadedPageSize = 25;
        component.LoadedFilter = new TestFilter { Email = "admin@example.test" };

        await component.SetRouteAsync(2, 25, new TestFilter { Email = "admin@example.test" });

        component.LoadRequests.ShouldBeEmpty();
        component.Page.ShouldBeSameAs(page);
    }

    [Theory(DisplayName = "FiltersEqual should compare values when filter instances differ")]
    [InlineData("admin@example.test", 18, 1)]
    [InlineData("other@example.test", 18, 2)]
    [InlineData("admin@example.test", 21, 2)]
    [InlineData(null, null, 2)]
    public async Task FiltersEqual_Should_CompareValues_When_FilterInstancesDiffer(
        string? email,
        int? minimumAge,
        int expectedLoads)
    {
        TestComponent component = CreateComponent();
        await component.SetRouteAsync(2, 25, new TestFilter { Email = "admin@example.test", MinimumAge = 18 });

        await component.SetRouteAsync(2, 25, new TestFilter { Email = email, MinimumAge = minimumAge });

        component.LoadRequests.Count.ShouldBe(expectedLoads);
    }

    [Fact(DisplayName = "LoadPageAsync should clear error when retry succeeds")]
    public async Task LoadPageAsync_Should_ClearError_When_RetrySucceeds()
    {
        TestComponent component = CreateComponent();
        component.OnLoad = static (_, _, _, _) => Task.FromResult(
            Result.Failure<PaginationResult<TestItem>>(
                Error.Unexpected("API is unavailable.")));

        await component.SetRouteAsync(1, 10);

        component.Error.ShouldBe("API is unavailable.");
        component.Page.ShouldBeNull();

        component.OnLoad = TestOperations.SuccessfulLoadAsync;
        await component.ReloadAsync();

        component.Error.ShouldBeNull();
        component.Page.ShouldNotBeNull();
    }

    [Fact(DisplayName = "IsCurrentLoad should ignore obsolete load when route state changes")]
    public async Task IsCurrentLoad_Should_IgnoreObsoleteLoad_When_RouteStateChanges()
    {
        var firstLoad = new TaskCompletionSource<Result<PaginationResult<TestItem>>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var secondLoad = new TaskCompletionSource<Result<PaginationResult<TestItem>>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        TestComponent component = CreateComponent();
        component.OnLoad = (pageNumber, _, _, _) =>
            pageNumber == 1 ? firstLoad.Task : secondLoad.Task;

        Task firstTask = component.SetRouteAsync(1, 10);
        Task secondTask = component.SetRouteAsync(2, 25);

        secondLoad.SetResult(Result.Success(CreatePage(2, 25)));
        await secondTask;
        firstLoad.SetResult(Result.Success(CreatePage(1, 10)));
        await firstTask;

        component.Page?.PageNumber.ShouldBe(2);
        component.Page?.PageSize.ShouldBe(25);
        component.Loading.ShouldBeFalse();
    }

    [Fact(DisplayName = "OnParametersSetAsync should stop loading when load throws")]
    public async Task OnParametersSetAsync_Should_StopLoading_When_LoadThrows()
    {
        TestComponent component = CreateComponent();
        component.OnLoad = static (_, _, _, _) =>
            throw new InvalidOperationException("Load failed.");

        await Should.ThrowAsync<InvalidOperationException>(
            () => component.SetRouteAsync(1, 10));

        component.Loading.ShouldBeFalse();
    }

    [Fact(DisplayName = "OnItemRemovedAsync should reload when page still contains items")]
    public async Task OnItemRemovedAsync_Should_Reload_When_PageStillContainsItems()
    {
        TestComponent component = CreateComponent();
        component.OnLoad = (pageNumber, pageSize, _, _) => Task.FromResult(
            Result.Success(new PaginationResult<TestItem>
            {
                Items = [new TestItem(), new TestItem()],
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 2,
                TotalPages = 1,
            }));

        await component.SetRouteAsync(1, 10);
        await component.NotifyItemRemovedAsync();

        component.LoadRequests.Count.ShouldBe(2);
    }

    private TestComponent CreateComponent()
    {
        return new TestComponent(
            Services.GetRequiredService<NavigationManager>());
    }

    private static PaginationResult<TestItem> CreatePage(int pageNumber, int pageSize)
    {
        return new PaginationResult<TestItem>
        {
            Items = [new TestItem()],
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 1,
            TotalPages = 1,
        };
    }

    private sealed class TestComponent : MudPagedListComponentBase<TestItem, TestFilter>
    {
        private readonly NavigationManager _navigation;
        private readonly TestOperations _operations;

        public TestComponent(NavigationManager navigation)
            : this(new TestOperations(), navigation)
        {
        }

        private TestComponent(
            TestOperations operations,
            NavigationManager navigation)
            : base(
                operations.LoadAsync,
                "/items",
                navigation)
        {
            _navigation = navigation;
            _operations = operations;
        }

        public Func<
            int,
            int,
            TestFilter,
            CancellationToken,
            Task<Result<PaginationResult<TestItem>>>> OnLoad
        {
            get => _operations.OnLoad;
            set => _operations.OnLoad = value;
        }

        public List<LoadRequest> LoadRequests => _operations.LoadRequests;

        public string? RouteEmail { get; private set; }

        public int? RouteMinimumAge { get; private set; }

        public string? Error => LoadError;

        public bool Loading => IsLoading;

        public int Pages => TotalPages;

        public long ItemsCount => TotalCount;

        public TestFilter DraftFilter => FilterDraft;

        public bool FilterIsActive => HasActiveFilter;

        public string CurrentUri => _navigation.Uri;

        protected override TestFilter AppliedFilter => new()
        {
            Email = RouteEmail,
            MinimumAge = RouteMinimumAge,
        };

        public Task SetRouteAsync(
            int? pageNumber,
            int? pageSize,
            TestFilter? filter = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            RouteEmail = filter?.Email;
            RouteMinimumAge = filter?.MinimumAge;
            return base.OnParametersSetAsync();
        }

        public Task ReloadAsync()
        {
            return LoadPageAsync();
        }

        public void ApplyDraftFilter()
        {
            ApplyFilter();
        }

        public void ResetDraftFilter()
        {
            ResetFilter();
        }

        public Task NotifyItemRemovedAsync()
        {
            return OnItemRemovedAsync();
        }

        public new void ChangePage(int pageNumber)
        {
            base.ChangePage(pageNumber);
        }

        public new void ChangePageSize(int pageSize)
        {
            base.ChangePageSize(pageSize);
        }

        public new void NavigateToList(TestFilter filter, int pageNumber, int pageSize)
        {
            base.NavigateToList(filter, pageNumber, pageSize);
        }
    }

    private sealed class TestOperations
    {
        public Func<
            int,
            int,
            TestFilter,
            CancellationToken,
            Task<Result<PaginationResult<TestItem>>>>
            OnLoad
        { get; set; } =
                SuccessfulLoadAsync;

        public List<LoadRequest> LoadRequests { get; } = [];

        public Task<Result<PaginationResult<TestItem>>> LoadAsync(
            int pageNumber,
            int pageSize,
            TestFilter filter,
            CancellationToken cancellationToken)
        {
            LoadRequests.Add(new LoadRequest(pageNumber, pageSize, filter));
            return OnLoad(pageNumber, pageSize, filter, cancellationToken);
        }

        public static Task<Result<PaginationResult<TestItem>>> SuccessfulLoadAsync(
            int pageNumber,
            int pageSize,
            TestFilter filter,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(CreatePage(pageNumber, pageSize)));
        }
    }

    private sealed class TestItem
    {
    }

    private sealed class TestFilter
    {
        public string? Email { get; set; }

        public int? MinimumAge { get; set; }
    }

    private sealed record LoadRequest(
        int PageNumber,
        int PageSize,
        TestFilter Filter);
}
