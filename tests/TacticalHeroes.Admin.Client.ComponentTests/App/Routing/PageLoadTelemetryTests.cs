using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

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
        void Content(RenderTreeBuilder builder)
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
        }
        var component = Render<PageLoadTelemetry>(parameters => parameters.Add(value => value.ChildContent, Content));
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

    [Fact(DisplayName = "OnParametersSet should provide page state when content is rendered")]
    public void OnParametersSet_Should_ProvidePageState_When_ContentIsRendered()
    {
        var component = Render<PageLoadTelemetry>(parameters => parameters.AddChildContent("Ready"));

        var state = component.FindComponent<CascadingValue<PageLoadState>>().Instance.Value.ShouldNotBeNull();

        state.IsLoading.ShouldBeFalse();
        state.HasError.ShouldBeFalse();
        component.Markup.ShouldContain("Ready");
    }

    [Theory(DisplayName = "OnParametersSet should isolate page state when address changes")]
    [InlineData("/users/1", "/users/2")]
    [InlineData("/users?page=1", "/users?page=2")]
    [InlineData("/users#first", "/users#second")]
    public async Task OnParametersSet_Should_IsolatePageState_When_AddressChanges(string previousUri, string currentUri)
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(previousUri);
        var component = Render<PageLoadTelemetry>(parameters => parameters
            .AddChildContent<PageLoadReporter>(reporter => reporter.Add(value => value.Loading, true)));
        var previousState = component.FindComponent<CascadingValue<PageLoadState>>().Instance.Value.ShouldNotBeNull();
        var previousLoader = component.FindComponent<PageLoadReporter>().Instance;

        await component.InvokeAsync(() => navigation.NavigateTo(currentUri));
        component.Render(parameters => parameters
            .AddChildContent<PageLoadReporter>(reporter => reporter.Add(value => value.Loading, true)));

        var currentState = component.FindComponent<CascadingValue<PageLoadState>>().Instance.Value.ShouldNotBeNull();
        currentState.ShouldNotBeSameAs(previousState);
        component.FindComponent<PageLoadReporter>().Instance.ShouldNotBeSameAs(previousLoader);
        previousState.IsLoading.ShouldBeFalse();
        currentState.IsLoading.ShouldBeTrue();
        currentState.HasError.ShouldBeFalse();

        int renderCount = component.RenderCount;
        using var lateLoader = Render<PageLoadReporter>(parameters => parameters
            .AddCascadingValue(previousState)
            .Add(value => value.HasError, true));

        previousState.HasError.ShouldBeTrue();
        currentState.IsLoading.ShouldBeTrue();
        currentState.HasError.ShouldBeFalse();
        component.RenderCount.ShouldBe(renderCount);
    }

    [Fact(DisplayName = "OnParametersSet should preserve page state when address is unchanged")]
    public void OnParametersSet_Should_PreservePageState_When_AddressIsUnchanged()
    {
        var component = Render<PageLoadTelemetry>(parameters => parameters
            .AddChildContent<PageLoadReporter>(reporter => reporter.Add(value => value.Loading, true)));
        var state = component.FindComponent<CascadingValue<PageLoadState>>().Instance.Value.ShouldNotBeNull();
        var loader = component.FindComponent<PageLoadReporter>().Instance;

        component.Render(parameters => parameters
            .AddChildContent<PageLoadReporter>(reporter => reporter.Add(value => value.Loading, false)));

        component.FindComponent<CascadingValue<PageLoadState>>().Instance.Value.ShouldBeSameAs(state);
        component.FindComponent<PageLoadReporter>().Instance.ShouldBeSameAs(loader);
        state.IsLoading.ShouldBeFalse();
        state.HasError.ShouldBeFalse();
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
