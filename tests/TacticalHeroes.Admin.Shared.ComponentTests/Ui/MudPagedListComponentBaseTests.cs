using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class MudPagedListComponentBaseTests : BunitContext
{
    [Fact(DisplayName = "Loads once for the same route state and reloads when it changes")]
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

        component.LoadRequests.ShouldBe(
        [
            new LoadRequest(2, 25, filter),
            new LoadRequest(3, 25, filter),
        ]);
        component.Page?.PageNumber.ShouldBe(3);
    }

    [Fact(DisplayName = "Exposes zero totals before a page is loaded")]
    public void Totals_Should_BeZero_When_PageIsUnavailable()
    {
        TestComponent component = CreateComponent();

        component.Pages.ShouldBe(0);
        component.ItemsCount.ShouldBe(0);
    }

    [Fact(DisplayName = "Applies and resets all filter fields from the first page")]
    public async Task FilterActions_Should_NavigateFromFirstPage_When_FilterChanges()
    {
        TestComponent component = CreateComponent();

        await component.SetRouteAsync(2, 25, new TestFilter
        {
            Email = "admin@example.test",
            MinimumAge = 18,
        });

        component.DraftFilter.ShouldBe(new TestFilter
        {
            Email = "admin@example.test",
            MinimumAge = 18,
        });
        component.FilterIsActive.ShouldBeTrue();

        component.DraftFilter.Email = "moderator@example.test";
        component.DraftFilter.MinimumAge = 21;
        component.ApplyDraftFilter();

        component.CurrentUri.ShouldEndWith(
            "/items?email=moderator%40example.test&minimumAge=21&pageSize=25");

        component.ResetDraftFilter();

        component.DraftFilter.ShouldBe(new TestFilter());
        component.CurrentUri.ShouldEndWith(
            "/items?pageSize=25");
    }

    [Fact(DisplayName = "Shows a load error and allows retrying the same route state")]
    public async Task ReloadAsync_Should_ClearError_When_RetrySucceeds()
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

    [Fact(DisplayName = "Does not apply an obsolete load after route state changes")]
    public async Task OnParametersSetAsync_Should_IgnoreObsoleteLoad_When_RouteStateChanges()
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

    [Fact(DisplayName = "Stops loading when the load operation throws")]
    public async Task OnParametersSetAsync_Should_StopLoading_When_LoadThrows()
    {
        TestComponent component = CreateComponent();
        component.OnLoad = static (_, _, _, _) =>
            throw new InvalidOperationException("Load failed.");

        await Should.ThrowAsync<InvalidOperationException>(
            () => component.SetRouteAsync(1, 10));

        component.Loading.ShouldBeFalse();
    }

    [Fact(DisplayName = "Reloads the current page after an item is removed")]
    public async Task OnItemRemovedAsync_Should_Reload_When_PageStillContainsItems()
    {
        TestComponent component = CreateComponent();
        component.OnLoad = (pageNumber, pageSize, _, _) => Task.FromResult(
            Result.Success(new PaginationResult<TestItem>(
                [new TestItem(), new TestItem()],
                pageNumber,
                pageSize,
                2,
                1)));

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
        return new PaginationResult<TestItem>(
            [new TestItem()],
            pageNumber,
            pageSize,
            1,
            1);
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

        public IReadOnlyList<LoadRequest> LoadRequests => _operations.LoadRequests;

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

        public async Task<Result<PaginationResult<TestItem>>> LoadAsync(
            int pageNumber,
            int pageSize,
            TestFilter filter,
            CancellationToken cancellationToken)
        {
            LoadRequests.Add(new LoadRequest(pageNumber, pageSize, filter));
            return await OnLoad(pageNumber, pageSize, filter, cancellationToken);
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

    private sealed record TestFilter
    {
        public string? Email { get; set; }

        public int? MinimumAge { get; set; }
    }

    private sealed record LoadRequest(
        int PageNumber,
        int PageSize,
        TestFilter Filter);
}
