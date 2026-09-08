using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Layout;

public sealed class PageHeaderTests : BunitContext
{
    public PageHeaderTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Render should show title and subtitle when subtitle provided")]
    public void Render_Should_ShowTitleAndSubtitle_When_SubtitleProvided()
    {
        var component = Render<PageHeader>(parameters => parameters
            .Add(header => header.Title, "Roles")
            .Add(header => header.Subtitle, "Manage application roles"));

        component.Find(".page-title").TextContent.Trim().ShouldBe("Roles");
        component.Find(".page-subtitle").TextContent.Trim().ShouldBe("Manage application roles");
    }

    [Fact(DisplayName = "Render should omit optional sections when they are not provided")]
    public void Render_Should_OmitOptionalSections_When_TheyAreNotProvided()
    {
        var component = Render<PageHeader>(parameters => parameters
            .Add(header => header.Title, "Roles"));

        component.FindAll(".page-subtitle").ShouldBeEmpty();
        component.FindAll(".page-actions").ShouldBeEmpty();
    }
}
