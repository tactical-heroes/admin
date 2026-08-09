using MudBlazor.Services;

using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class ListFiltersTests : BunitContext
{
    public ListFiltersTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Disables actions when filters are unavailable")]
    public void FilterActions_Should_BeDisabled_When_FiltersAreUnavailable()
    {
        var component = Render<ListFilters>(parameters => parameters
            .Add(filters => filters.Expanded, true));

        FindButton(component, "Применить").HasAttribute("disabled").ShouldBeTrue();
        FindButton(component, "Сбросить").HasAttribute("disabled").ShouldBeTrue();
        component.Markup.ShouldContain("Для этого списка дополнительные фильтры не предусмотрены.");
    }

    private static AngleSharp.Dom.IElement FindButton(
        IRenderedComponent<ListFilters> component,
        string text)
    {
        return component.FindAll("button")
            .Single(button => button.TextContent.Contains(text, StringComparison.Ordinal));
    }
}
