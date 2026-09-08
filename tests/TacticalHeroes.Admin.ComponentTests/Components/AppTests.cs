using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using TacticalHeroes.Admin.Client.App.Routing;

using AppComponent = TacticalHeroes.Admin.Components.App;

namespace TacticalHeroes.Admin.ComponentTests.Components;

public sealed class AppTests : BunitContext
{
    [Fact(DisplayName = "The application document wires its language, assets, routing, and reconnect surface")]
    public void Render_Should_ComposeDocument_When_ApplicationIsRendered()
    {
        ComponentFactories.AddStub<Routes>();
        ComponentFactories.AddStub<HeadOutlet>();
        ComponentFactories.AddStub<ResourcePreloader>();
        ComponentFactories.AddStub<ImportMap>();

        var component = Render<AppComponent>();

        component.Find("html").GetAttribute("lang").ShouldBe("ru");
        component.Find("base").GetAttribute("href").ShouldBe("/");
        component.FindAll("script").Select(script => script.GetAttribute("src"))
            .ShouldContain("_framework/blazor.web.js");
        component.Find("#components-reconnect-modal").ShouldNotBeNull();
    }
}
