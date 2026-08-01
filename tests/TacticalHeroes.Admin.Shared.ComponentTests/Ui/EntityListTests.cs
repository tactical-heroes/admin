using MudBlazor;
using MudBlazor.Services;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class EntityListTests : BunitContext
{
    public EntityListTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Keeps filters collapsed until the user expands them")]
    public void Filters_Should_BeCollapsed_When_ListIsRendered()
    {
        var component = RenderList(hasFilters: true);

        component.FindComponent<MudCollapse>().Instance.Expanded.ShouldBeFalse();

        FindButton(component, "Фильтры").Click();

        component.FindComponent<MudCollapse>().Instance.Expanded.ShouldBeTrue();
    }

    [Fact(DisplayName = "Invokes filter actions when filters are available")]
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

    private IRenderedComponent<EntityList<string>> RenderList(
        bool hasFilters,
        bool hasActiveFilters = false,
        Action? onApply = null,
        Action? onReset = null)
    {
        return Render<EntityList<string>>(parameters => parameters
            .Add(list => list.Items, ["Запись"])
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
            .Add(list => list.HeaderContent, builder => builder.AddMarkupContent(0, "<th>Название</th>"))
            .Add(list => list.RowTemplate, item => builder =>
                builder.AddMarkupContent(0, $"<td>{item}</td>")));
    }

    private static AngleSharp.Dom.IElement FindButton(
        IRenderedComponent<EntityList<string>> component,
        string text)
    {
        return component.FindAll("button")
            .Single(button => button.TextContent.Contains(text, StringComparison.Ordinal));
    }
}
