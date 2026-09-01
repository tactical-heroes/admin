using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Model;

public sealed class PaginationResultTests
{
    [Fact(DisplayName = "Creates an empty page with the requested position")]
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
