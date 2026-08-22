using MudBlazor.Services;

using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class LoadableContentTests : BunitContext
{
    public LoadableContentTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Shows loading content while loading")]
    public void Render_Should_ShowLoadingContent_When_Loading()
    {
        var component = Render<LoadableContent>(parameters => parameters
            .Add(content => content.Loading, true)
            .Add(content => content.LoadError, "Load failed.")
            .Add(content => content.OnRetry, () => { })
            .Add(content => content.LoadingContent, builder =>
                builder.AddMarkupContent(0, "<p class=\"loading\">Loading</p>"))
            .AddChildContent("<p class=\"content\">Content</p>"));

        component.Find(".loading").TextContent.ShouldBe("Loading");
        component.FindAll(".content").ShouldBeEmpty();
        component.Markup.ShouldNotContain("Load failed.");
    }

    [Fact(DisplayName = "Shows an error and retries loading")]
    public void Render_Should_InvokeRetry_When_ErrorIsShown()
    {
        int retryCount = 0;
        var component = Render<LoadableContent>(parameters => parameters
            .Add(content => content.LoadError, "Load failed.")
            .Add(content => content.OnRetry, () => retryCount++)
            .Add(content => content.LoadingContent, builder =>
                builder.AddMarkupContent(0, "<p>Loading</p>"))
            .AddChildContent("<p class=\"content\">Content</p>"));

        component.Find("button").Click();

        component.Markup.ShouldContain("Load failed.");
        retryCount.ShouldBe(1);
        component.FindAll(".content").ShouldBeEmpty();
    }

    [Fact(DisplayName = "Shows child content after loading succeeds")]
    public void Render_Should_ShowChildContent_When_LoadSucceeds()
    {
        var component = Render<LoadableContent>(parameters => parameters
            .Add(content => content.OnRetry, () => { })
            .Add(content => content.LoadingContent, builder =>
                builder.AddMarkupContent(0, "<p>Loading</p>"))
            .AddChildContent("<p class=\"content\">Content</p>"));

        component.Find(".content").TextContent.ShouldBe("Content");
    }
}
