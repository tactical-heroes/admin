using MudBlazor.Services;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class PageHeaderTests : BunitContext
{
    public PageHeaderTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Renders title and subtitle when subtitle is provided")]
    public void Render_Should_ShowTitleAndSubtitle_When_SubtitleProvided()
    {
        var component = Render<PageHeader>(parameters => parameters
            .Add(header => header.Title, "Roles")
            .Add(header => header.Subtitle, "Manage application roles"));

        component.Find(".page-title").TextContent.Trim().ShouldBe("Roles");
        component.Find(".page-subtitle").TextContent.Trim().ShouldBe("Manage application roles");
    }

    [Fact(DisplayName = "Omits optional sections when they are not provided")]
    public void Render_Should_OmitOptionalSections_When_TheyAreNotProvided()
    {
        var component = Render<PageHeader>(parameters => parameters
            .Add(header => header.Title, "Roles"));

        component.FindAll(".page-subtitle").ShouldBeEmpty();
        component.FindAll(".page-actions").ShouldBeEmpty();
    }
}
