using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Layout;

public sealed class PageBackButtonTests : BunitContext
{
    public PageBackButtonTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Render should link to provided list when list route is provided")]
    public void Render_Should_LinkToProvidedList_When_ListRouteIsProvided()
    {
        var component = Render<PageBackButton>(parameters => parameters
            .Add(button => button.Href, "/roles"));

        component.Find("a").GetAttribute("href").ShouldBe("/roles");
        component.Find("a").TextContent.Trim().ShouldBe("К списку");
    }
}
