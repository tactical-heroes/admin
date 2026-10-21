using MudBlazor;
using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Inputs;

public sealed class DebouncedTextFilterTests : BunitContext
{
    public DebouncedTextFilterTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "ApplyFilterAsync should apply filter when minimum length is reached")]
    public async Task ApplyFilterAsync_Should_ApplyFilter_When_MinimumLengthIsReached()
    {
        int applyCount = 0;
        IRenderedComponent<DebouncedTextFilter> component = RenderFilter(
            () => applyCount++);
        MudTextField<string> textField = component.FindComponent<MudTextField<string>>()
            .Instance;

        await component.InvokeAsync(
            () => textField.OnDebounceIntervalElapsed.InvokeAsync("ab"));
        await component.InvokeAsync(
            () => textField.OnDebounceIntervalElapsed.InvokeAsync("abc"));
        await component.InvokeAsync(
            () => textField.OnDebounceIntervalElapsed.InvokeAsync(string.Empty));

        applyCount.ShouldBe(2);
    }

    [Fact(DisplayName = "Render should display assigned text when filter has a value")]
    public void Render_Should_DisplayAssignedText_When_FilterHasAValue()
    {
        var component = Render<DebouncedTextFilter>(parameters => parameters
            .Add(filter => filter.Label, "Search")
            .Add(filter => filter.Value, "dragon"));

        component.Find("input").GetAttribute("value").ShouldBe("dragon");
        component.Render(parameters => parameters.Add(filter => filter.Value, "knight"));
        component.Find("input").GetAttribute("value").ShouldBe("knight");
    }

    private IRenderedComponent<DebouncedTextFilter> RenderFilter(Action onFilterChanged)
    {
        return Render<DebouncedTextFilter>(parameters => parameters
            .Add(filter => filter.Label, "Search")
            .Add(filter => filter.MinimumLength, 3)
            .Add(filter => filter.OnFilterChanged, onFilterChanged));
    }
}
