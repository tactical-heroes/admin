using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Model;

public sealed class PagedListStateTests
{
    [Fact(DisplayName = "Stores the loaded page and its route state")]
    public async Task LoadAsync_Should_StorePage_When_RequestSucceeds()
    {
        var state = new PagedListState<string>();
        var page = new PaginationResult<string>(["Role"], 2, 25, 26, 2);

        await state.LoadAsync(
            2,
            25,
            _ =>
            {
                state.Loading.ShouldBeTrue();
                return Task.FromResult(Result.Success(page));
            },
            Xunit.TestContext.Current.CancellationToken,
            "admin@example.test");

        state.Page.ShouldBeSameAs(page);
        state.LoadError.ShouldBeNull();
        state.Loading.ShouldBeFalse();
        state.Matches(2, 25, "ADMIN@example.test").ShouldBeTrue();
    }

    [Fact(DisplayName = "Stores load errors and always resets the loading flag")]
    public async Task LoadAsync_Should_StoreError_When_RequestFails()
    {
        var state = new PagedListState<string>();

        await state.LoadAsync(
            1,
            10,
            _ => Task.FromResult(Result.Failure<PaginationResult<string>>(
                Error.Unexpected("API is unavailable."))),
            Xunit.TestContext.Current.CancellationToken);

        state.Page.ShouldBeNull();
        state.LoadError.ShouldBe("API is unavailable.");
        state.Loading.ShouldBeFalse();
    }

    [Fact(DisplayName = "Tracks the deleting item only while delete is running")]
    public async Task DeleteAsync_Should_ResetDeletingId_When_RequestCompletes()
    {
        var state = new PagedListState<string>();
        Guid id = Guid.NewGuid();

        Result result = await state.DeleteAsync(
            id,
            _ =>
            {
                state.DeletingId.ShouldBe(id);
                return Task.FromResult(Result.Success());
            },
            Xunit.TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        state.DeletingId.ShouldBeNull();
    }
}
