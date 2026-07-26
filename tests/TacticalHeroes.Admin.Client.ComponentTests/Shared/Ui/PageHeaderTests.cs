using MudBlazor.Services;
using TacticalHeroes.Admin.Client.Shared.Ui;

namespace TacticalHeroes.Admin.Client.ComponentTests.Shared.Ui;

public sealed class PageHeaderTests : BunitContext
{
    public PageHeaderTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Render_Should_ShowTitleAndSubtitle_When_SubtitleProvided()
    {
        var component = Render<PageHeader>(parameters => parameters
            .Add(header => header.Title, "Roles")
            .Add(header => header.Subtitle, "Manage application roles"));

        component.Find(".page-title").TextContent.Trim().ShouldBe("Roles");
        component.Find(".page-subtitle").TextContent.Trim().ShouldBe("Manage application roles");
    }

    [Fact]
    public void Render_Should_OmitOptionalSections_When_TheyAreNotProvided()
    {
        var component = Render<PageHeader>(parameters => parameters
            .Add(header => header.Title, "Roles"));

        component.FindAll(".page-subtitle").ShouldBeEmpty();
        component.FindAll(".page-actions").ShouldBeEmpty();
    }
}
