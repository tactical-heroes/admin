using MudBlazor;
using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Lists;

public sealed class EntityListTests : BunitContext
{
    public EntityListTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "ToggleFilters should toggle expansion when button is clicked")]
    public void ToggleFilters_Should_ToggleExpansion_When_ButtonIsClicked()
    {
        var component = RenderList(hasFilters: true);

        component.FindComponent<ListFilters>().Instance.Expanded.ShouldBeFalse();

        FindButton(component, "Фильтры").Click();

        component.FindComponent<ListFilters>().Instance.Expanded.ShouldBeTrue();

        FindButton(component, "Фильтры").Click();

        component.FindComponent<ListFilters>().Instance.Expanded.ShouldBeFalse();
    }

    [Fact(DisplayName = "FilterActions should invoke callbacks when filters are available")]
    public void FilterActions_Should_InvokeCallbacks_When_FiltersAreAvailable()
    {
        var applied = false;
        var reset = false;
        var component = RenderList(
            hasFilters: true,
            hasActiveFilters: true,
            onApply: () => applied = true,
            onReset: () => reset = true);

        FindButton(component, "Фильтры").Click();
        FindButton(component, "Применить").Click();
        FindButton(component, "Сбросить").Click();

        applied.ShouldBeTrue();
        reset.ShouldBeTrue();
    }

    [Fact(DisplayName = "ResetSortingAsync should clear criteria when reset is clicked")]
    public void ResetSortingAsync_Should_ClearCriteria_When_ResetIsClicked()
    {
        string[] sorting = ["Name:asc"];
        var component = RenderList(hasFilters: false);
        component.Render(parameters => parameters
            .Add(list => list.Sorting, sorting)
            .Add(list => list.OnSortingChanged, value => sorting = value));

        FindButton(component, "Сбросить сортировку").Click();

        sorting.ShouldBeEmpty();
    }

    [Fact(DisplayName = "OnParametersSet should restore criteria when sorting contains mixed casing and whitespace")]
    public void OnParametersSet_Should_RestoreCriteria_When_SortingContainsMixedCasingAndWhitespace()
    {
        var component = RenderList(hasFilters: false);

        component.Render(parameters => parameters.Add(list => list.Sorting, [" NAME : DESC ", "Id:asc"]));

        var grid = component.FindComponent<MudDataGrid<TestRow>>().Instance;
        grid.SortDefinitions["Name"].Descending.ShouldBeTrue();
        grid.SortDefinitions["Id"].Index.ShouldBe(1);
        component.Find(".mud-direction-desc").ShouldNotBeNull();
    }

    [Fact(DisplayName = "GetGridPageAsync should preserve server order and all rows when page is supplied")]
    public void GetGridPageAsync_Should_PreserveServerOrderAndAllRows_When_PageIsSupplied()
    {
        var component = RenderList(hasFilters: false);
        TestRow[] rows = Enumerable.Range(1, 25).Select(id => new TestRow(id, $"Name {26 - id}")).ToArray();

        component.Render(parameters => parameters
            .Add(list => list.Items, rows)
            .Add(list => list.PageNumber, 3)
            .Add(list => list.PageSize, 25)
            .Add(list => list.TotalPages, 4)
            .Add(list => list.TotalCount, 100)
            .Add(list => list.Sorting, ["Name:asc"]));

        component.WaitForAssertion(() =>
        {
            component.FindAll("tbody tr").Count.ShouldBe(25);
            component.FindComponent<MudDataGrid<TestRow>>().Instance.ServerItems.ShouldBe(rows);
            component.FindComponent<ListPagination>().Instance.PageNumber.ShouldBe(3);
            component.FindComponent<ListPagination>().Instance.TotalCount.ShouldBe(100);
        });
    }

    [Fact(DisplayName = "OnAfterRenderAsync should restore priorities without notifications when url sorting changes")]
    public void OnAfterRenderAsync_Should_RestorePrioritiesWithoutNotifications_When_UrlSortingChanges()
    {
        var notifications = 0;
        var component = RenderList(hasFilters: false);

        component.Render(parameters => parameters
            .Add(list => list.Sorting, ["Name:desc", "Id:asc"])
            .Add(list => list.OnSortingChanged, _ => notifications++));

        component.WaitForAssertion(() =>
        {
            component.FindAll(".mud-sort-index:not(.invisible)").Select(element => element.TextContent.Trim())
                .ShouldBe(["1", "2"]);
            notifications.ShouldBe(0);
        });
        component.Render(parameters => parameters.Add(list => list.Sorting, Array.Empty<string>()));
        component.FindAll(".mud-direction-desc, .mud-direction-asc").ShouldBeEmpty();
        notifications.ShouldBe(0);
    }

    [Fact(DisplayName = "ChangeSortingAsync should preserve priority when ctrl click adds another column")]
    public void ChangeSortingAsync_Should_PreservePriority_When_CtrlClickAddsAnotherColumn()
    {
        string[] sorting = [];
        var component = RenderList(hasFilters: false);
        component.Render(parameters => parameters.Add(list => list.OnSortingChanged, value => sorting = value));

        component.FindAll(".sortable-column-header")[0].Click();
        sorting.ShouldBe(["Name:asc"]);
        component.Render(parameters => parameters.Add(list => list.Sorting, sorting));
        component.FindAll(".sortable-column-header")[1].Click(new Microsoft.AspNetCore.Components.Web.MouseEventArgs { CtrlKey = true });

        sorting.ShouldBe(["Name:asc", "Id:asc"]);
        component.Render(parameters => parameters.Add(list => list.Sorting, sorting));
        component.FindAll(".sortable-column-header")[0].Click(new Microsoft.AspNetCore.Components.Web.MouseEventArgs { AltKey = true });
        sorting.ShouldBe(["Id:asc"]);
    }

    public sealed record TestRow(int Id, string Name);

    private IRenderedComponent<EntityList<TestRow>> RenderList(
        bool hasFilters,
        bool hasActiveFilters = false,
        Action? onApply = null,
        Action? onReset = null)
    {
        return Render<EntityList<TestRow>>(parameters => parameters
            .Add(list => list.Items, [new TestRow(1, "Запись")])
            .Add(list => list.EmptyText, "Нет записей")
            .Add(list => list.RefreshLabel, "Обновить список")
            .Add(list => list.HasFilters, hasFilters)
            .Add(list => list.HasActiveFilters, hasActiveFilters)
            .Add(list => list.Filters, builder => builder.AddContent(0, "Поле фильтра"))
            .Add(list => list.OnApplyFilters, onApply ?? (() => { }))
            .Add(list => list.OnResetFilters, onReset ?? (() => { }))
            .Add(list => list.OnRefresh, () => { })
            .Add(list => list.OnPageNumberChanged, _ => { })
            .Add(list => list.OnPageSizeChanged, _ => { })
            .Add(list => list.Columns, builder =>
            {
                builder.OpenComponent<PropertyColumn<TestRow, string>>(0);
                builder.AddAttribute(1, "Property", (System.Linq.Expressions.Expression<Func<TestRow, string>>)(item => item.Name));
                builder.AddAttribute(2, "Title", "Название");
                builder.CloseComponent();
                builder.OpenComponent<PropertyColumn<TestRow, int>>(3);
                builder.AddAttribute(4, "Property", (System.Linq.Expressions.Expression<Func<TestRow, int>>)(item => item.Id));
                builder.AddAttribute(5, "Title", "Id");
                builder.CloseComponent();
            }));
    }

    private static AngleSharp.Dom.IElement FindButton(
        IRenderedComponent<EntityList<TestRow>> component,
        string text)
    {
        return component.FindAll("button")
            .Single(button => button.TextContent.Contains(text, StringComparison.Ordinal));
    }
}
