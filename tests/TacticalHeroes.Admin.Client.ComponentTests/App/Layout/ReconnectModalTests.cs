using TacticalHeroes.Admin.Client.App.Layout;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Layout;

public sealed class ReconnectModalTests : BunitContext
{
    [Fact(DisplayName = "Render should show recovery controls when connection ui is rendered")]
    public void Render_Should_ShowRecoveryControls_When_ConnectionUiIsRendered()
    {
        var component = Render<ReconnectModal>();

        component.Find("script").GetAttribute("src").ShouldEndWith("App/Layout/ReconnectModal.razor.js");
        component.Find("#components-reconnect-button").TextContent.Trim().ShouldBe("Повторить");
        component.Find("#components-resume-button").TextContent.Trim().ShouldBe("Продолжить");
        component.Find("#components-seconds-to-next-attempt").ShouldNotBeNull();
    }
}
