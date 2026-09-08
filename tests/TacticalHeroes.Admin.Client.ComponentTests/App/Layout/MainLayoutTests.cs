using TacticalHeroes.Admin.Client.App.Layout;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Layout;

public sealed class MainLayoutTests : ClientComponentTestContext
{
    [Fact(DisplayName = "The layout toggles the sidebar while preserving its page body")]
    public void ToggleDrawer_Should_ToggleSidebar_When_MenuIsClicked()
    {
        ComponentFactories.AddStub<MudBlazor.MudDrawer>();
        var component = Render<MainLayout>(parameters => parameters.Add(layout => layout.Body, "<h1>Page body</h1>"));

        component.Find("button[aria-label='Открыть меню']").Click();

        component.FindComponent<AdminSidebar>().Instance.Open.ShouldBeFalse();
        component.Find("button[aria-label='Открыть меню']").Click();

        component.FindComponent<AdminSidebar>().Instance.Open.ShouldBeTrue();
        component.Find("main h1").TextContent.ShouldBe("Page body");
    }
}
