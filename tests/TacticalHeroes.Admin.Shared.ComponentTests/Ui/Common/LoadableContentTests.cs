using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Common;

public sealed class LoadableContentTests : BunitContext
{
    [Theory(DisplayName = "Render should report page loading and errors when load changes")]
    [InlineData(true, "Previous attempt failed", true, false)]
    [InlineData(false, null, false, false)]
    [InlineData(false, "Load failed", false, true)]
    public void Render_Should_ReportPageState_When_LoadChanges(bool loading, string? error, bool expectedLoading, bool expectedError)
    {
        var state = new PageLoadState();

        Render<LoadableContent>(parameters => parameters
            .AddCascadingValue(state)
            .Add(content => content.Loading, loading)
            .Add(content => content.LoadError, error)
            .Add(content => content.OnRetry, () => { })
            .AddChildContent("<p>Ready</p>"));

        state.IsLoading.ShouldBe(expectedLoading);
        state.HasError.ShouldBe(expectedError);
    }

    public LoadableContentTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Render should show loading content when loading")]
    public void Render_Should_ShowLoadingContent_When_Loading()
    {
        var component = Render<LoadableContent>(parameters => parameters
            .Add(content => content.Loading, true)
            .Add(content => content.LoadError, "Load failed.")
            .Add(content => content.OnRetry, () => { })
            .AddChildContent("<p class=\"content\">Content</p>"));

        component.Find(".mud-skeleton")
            .GetAttribute("style")!
            .ShouldContain("height:360px");
        component.FindAll(".content").ShouldBeEmpty();
        component.Markup.ShouldNotContain("Load failed.");
    }

    [Fact(DisplayName = "Render should use loading height when provided")]
    public void Render_Should_UseLoadingHeight_When_Provided()
    {
        var component = Render<LoadableContent>(parameters => parameters
            .Add(content => content.Loading, true)
            .Add(content => content.LoadingHeight, "280px")
            .Add(content => content.OnRetry, () => { })
            .AddChildContent("<p>Content</p>"));

        component.Find(".mud-skeleton")
            .GetAttribute("style")!
            .ShouldContain("height:280px");
    }

    [Fact(DisplayName = "Render should invoke retry when error is shown")]
    public void Render_Should_InvokeRetry_When_ErrorIsShown()
    {
        int retryCount = 0;
        var component = Render<LoadableContent>(parameters => parameters
            .Add(content => content.LoadError, "Load failed.")
            .Add(content => content.OnRetry, () => retryCount++)
            .AddChildContent("<p class=\"content\">Content</p>"));

        component.Find("button").Click();

        component.Markup.ShouldContain("Load failed.");
        retryCount.ShouldBe(1);
        component.FindAll(".content").ShouldBeEmpty();
    }

    [Fact(DisplayName = "Render should show child content when load succeeds")]
    public void Render_Should_ShowChildContent_When_LoadSucceeds()
    {
        var component = Render<LoadableContent>(parameters => parameters
            .Add(content => content.OnRetry, () => { })
            .AddChildContent("<p class=\"content\">Content</p>"));

        component.Find(".content").TextContent.ShouldBe("Content");
    }
}
