using MudBlazor;
using MudBlazor.Services;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class ListPaginationTests : BunitContext
{
    public ListPaginationTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Shows page summary and configured navigation range")]
    public void Render_Should_ShowSummaryAndRange_When_PageIsProvided()
    {
        var component = Render<ListPagination>(parameters => parameters
            .Add(pagination => pagination.PageNumber, 3)
            .Add(pagination => pagination.PageSize, 10)
            .Add(pagination => pagination.TotalPages, 8)
            .Add(pagination => pagination.TotalCount, 73)
            .Add(pagination => pagination.VisibleItemCount, 10)
            .Add(pagination => pagination.PageNumberChanged, _ => { })
            .Add(pagination => pagination.PageSizeChanged, _ => { }));

        component.Markup.ShouldContain("Страница 3 из 8");
        component.Markup.ShouldContain("Показано 10 из 73");

        component.FindComponents<MudPagination>().Count.ShouldBe(1);
    }
}
