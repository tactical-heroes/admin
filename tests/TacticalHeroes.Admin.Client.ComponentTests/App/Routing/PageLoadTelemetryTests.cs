using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Shared.Ui.Common;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Routing;

public sealed class PageLoadTelemetryTests : ClientComponentTestContext
{
    [Fact(DisplayName = "OnStateChanged should observe nested loaders when required data finishes")]
    public void OnStateChanged_Should_ObserveNestedLoaders_When_RequiredDataFinishes()
    {
        bool loading = true;
        bool optionsLoading = true;
        RenderFragment content = builder =>
        {
            builder.OpenComponent<LoadableContent>(0);
            builder.AddAttribute(1, nameof(LoadableContent.Loading), loading);
            builder.AddAttribute(2, nameof(LoadableContent.OnRetry), EventCallback.Factory.Create(this, () => { }));
            builder.AddAttribute(3, nameof(LoadableContent.ChildContent), (RenderFragment)(child =>
            {
                child.OpenComponent<LoadableContent>(0);
                child.AddAttribute(1, nameof(LoadableContent.Loading), optionsLoading);
                child.AddAttribute(2, nameof(LoadableContent.OnRetry), EventCallback.Factory.Create(this, () => { }));
                child.AddAttribute(3, nameof(LoadableContent.ChildContent), (RenderFragment)(ready =>
                    ready.AddContent(0, "Data and options ready")));
                child.CloseComponent();
            }));
            builder.CloseComponent();
        };
        var component = Render<PageLoadTelemetry>(parameters => parameters.Add(value => value.ChildContent, content));
        var state = component.FindComponent<CascadingValue<PageLoadState>>().Instance.Value.ShouldNotBeNull();
        state.IsLoading.ShouldBeTrue();

        loading = false;
        component.Render();
        component.WaitForAssertion(() => state.IsLoading.ShouldBeTrue());
        optionsLoading = false;
        component.Render();

        component.WaitForAssertion(() =>
        {
            state.IsLoading.ShouldBeFalse();
            state.HasError.ShouldBeFalse();
            component.Markup.ShouldContain("Data and options ready");
        });
    }

    [Fact(DisplayName = "OnInitialized should provide page state when content is rendered")]
    public void OnInitialized_Should_ProvidePageState_When_ContentIsRendered()
    {
        var component = Render<PageLoadTelemetry>(parameters => parameters.AddChildContent("Ready"));

        var state = component.FindComponent<CascadingValue<PageLoadState>>().Instance.Value.ShouldNotBeNull();

        state.IsLoading.ShouldBeFalse();
        state.HasError.ShouldBeFalse();
        component.Markup.ShouldContain("Ready");
    }

    [Fact(DisplayName = "Dispose should stop observing when component is disposed")]
    public void Dispose_Should_StopObserving_When_ComponentIsDisposed()
    {
        var component = Render<PageLoadTelemetry>();
        var state = component.FindComponent<CascadingValue<PageLoadState>>().Instance.Value.ShouldNotBeNull();
        var reporter = Render<PageLoadReporter>(parameters => parameters.AddCascadingValue(state));
        component.Instance.Dispose();
        int renderCount = component.RenderCount;

        reporter.Render(parameters => parameters.Add(value => value.Loading, true));

        state.IsLoading.ShouldBeTrue();
        component.RenderCount.ShouldBe(renderCount);
    }
}
