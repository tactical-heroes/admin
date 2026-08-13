using MudBlazor.Services;

using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class PageBackButtonTests : BunitContext
{
    public PageBackButtonTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Renders a link back to the list")]
    public void Render_Should_LinkToProvidedList()
    {
        var component = Render<PageBackButton>(parameters => parameters
            .Add(button => button.Href, "/roles"));

        component.Find("a").GetAttribute("href").ShouldBe("/roles");
        component.Find("a").TextContent.Trim().ShouldBe("К списку");
    }
}
