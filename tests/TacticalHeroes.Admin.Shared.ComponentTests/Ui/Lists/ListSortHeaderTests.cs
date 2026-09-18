using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Lists;

public sealed class ListSortHeaderTests : BunitContext
{
    public ListSortHeaderTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "ToggleAsync should cycle sorting and preserve priority when other columns are sorted")]
    public void ToggleAsync_Should_CycleSortingAndPreservePriority_When_OtherColumnsAreSorted()
    {
        string[] sorting = ["Email:asc"];
        var component = Render<ListSortHeader>(parameters => parameters
            .Add(header => header.Field, "Name")
            .Add(header => header.Label, "Имя")
            .Add(header => header.Sorting, sorting)
            .Add(header => header.SortingChanged, value => sorting = value));

        component.Find("button").Click();
        sorting.ShouldBe(["Email:asc", "Name:asc"]);
        component.Render(parameters => parameters.Add(header => header.Sorting, sorting));
        component.Find(".sort-priority").TextContent.ShouldBe("2");
        component.Find("button").Click();
        sorting.ShouldBe(["Email:asc", "Name:desc"]);
        component.Render(parameters => parameters.Add(header => header.Sorting, sorting));
        component.Find("button").Click();
        sorting.ShouldBe(["Email:asc"]);
    }

    [Fact(DisplayName = "Render should restore direction when sorting uses mixed case and implicit ascending")]
    public void Render_Should_RestoreDirection_When_SortingUsesMixedCaseAndImplicitAscending()
    {
        var component = Render<ListSortHeader>(parameters => parameters
            .Add(header => header.Field, "Name")
            .Add(header => header.Label, "Имя")
            .Add(header => header.Sorting, ["name"])
            .Add(header => header.SortingChanged, _ => { }));

        component.Find("th").GetAttribute("aria-sort").ShouldBe("ascending");
        component.Render(parameters => parameters.Add(header => header.Sorting, [" NAME : DESC "]));
        component.Find("th").GetAttribute("aria-sort").ShouldBe("descending");
    }
}
