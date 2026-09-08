using MudBlazor;
using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Lists;

public sealed class ListPaginationTests : BunitContext
{
    [Fact(DisplayName = "ChangePageSizeAsync should notify parent when size changes")]
    public async Task ChangePageSizeAsync_Should_NotifyParent_When_SizeChanges()
    {
        int selectedSize = 0;
        var component = Render<ListPagination>(parameters => parameters
            .Add(pagination => pagination.PageSizeChanged, value => selectedSize = value)
            .Add(pagination => pagination.PageNumberChanged, _ => { }));

        await component.InvokeAsync(() =>
            component.FindComponent<MudSelect<int>>().Instance.ValueChanged.InvokeAsync(25));

        selectedSize.ShouldBe(25);
    }

    public ListPaginationTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Render should show summary and range when page is provided")]
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
