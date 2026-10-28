using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Common;

public sealed class PageLoadReporterTests : BunitContext
{
    [Fact(DisplayName = "OnParametersSet should wait for all loaders and preserve errors when states change")]
    public void OnParametersSet_Should_WaitForAllLoaders_When_StatesChange()
    {
        var state = new PageLoadState();
        int notifications = 0;
        state.Changed += () => notifications++;
        state.IsLoading.ShouldBeFalse();
        state.HasError.ShouldBeFalse();
        var page = Render<PageLoadReporter>(parameters => parameters
            .AddCascadingValue(state)
            .Add(reporter => reporter.Loading, true));
        var options = Render<PageLoadReporter>(parameters => parameters
            .AddCascadingValue(state)
            .Add(reporter => reporter.Loading, true));

        page.Render(parameters => parameters.Add(reporter => reporter.Loading, false));
        state.IsLoading.ShouldBeTrue();
        state.HasError.ShouldBeFalse();
        options.Render(parameters => parameters
            .Add(reporter => reporter.Loading, false)
            .Add(reporter => reporter.HasError, true));
        state.IsLoading.ShouldBeFalse();
        state.HasError.ShouldBeTrue();
        int beforeUnchangedRender = notifications;
        options.Render();
        notifications.ShouldBe(beforeUnchangedRender);
        options.Instance.Dispose();
        options.Instance.Dispose();

        state.IsLoading.ShouldBeFalse();
        state.HasError.ShouldBeFalse();
        notifications.ShouldBe(beforeUnchangedRender + 1);
    }

    [Fact(DisplayName = "OnParametersSet should detach from the previous page when context changes")]
    public void OnParametersSet_Should_DetachFromPreviousPage_When_ContextChanges()
    {
        var previous = new PageLoadState();
        var current = new PageLoadState();
        var component = Render<CascadingValue<PageLoadState>>(parameters => parameters
            .Add(value => value.Value, previous)
            .AddChildContent<PageLoadReporter>(reporter => reporter
                .Add(value => value.Loading, true)));
        previous.IsLoading.ShouldBeTrue();

        component.Render(parameters => parameters
            .Add(value => value.Value, current)
            .AddChildContent<PageLoadReporter>(reporter => reporter
                .Add(value => value.Loading, true)));

        previous.IsLoading.ShouldBeFalse();
        current.IsLoading.ShouldBeTrue();
        component.FindComponent<PageLoadReporter>().Instance.Dispose();
        current.IsLoading.ShouldBeFalse();
    }

    [Fact(DisplayName = "Dispose should be harmless when no scope is provided")]
    public void Dispose_Should_BeHarmless_When_NoScopeIsProvided()
    {
        var component = Render<PageLoadReporter>(parameters => parameters
            .Add(reporter => reporter.Loading, true)
            .Add(reporter => reporter.HasError, true));

        component.Markup.ShouldBeEmpty();
        Should.NotThrow(component.Instance.Dispose);
    }
}
