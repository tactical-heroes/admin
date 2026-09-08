namespace TacticalHeroes.Admin.Shared.ComponentTests.Model;

public sealed class PaginationResultTests
{
    [Fact(DisplayName = "Empty should create empty page when position is provided")]
    public void Empty_Should_CreateEmptyPage_When_PositionIsProvided()
    {
        PaginationResult<string> page = PaginationResult<string>.Empty(3, 25);

        page.Items.ShouldBeEmpty();
        page.PageNumber.ShouldBe(3);
        page.PageSize.ShouldBe(25);
        page.TotalCount.ShouldBe(0);
        page.TotalPages.ShouldBe(0);
    }
}
