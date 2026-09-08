using TacticalHeroes.Admin.Client.App.Layout;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Layout;

public sealed class ReconnectModalTests : BunitContext
{
    [Fact(DisplayName = "The reconnect surface exposes retry and resume controls with its script module")]
    public void Render_Should_ShowRecoveryControls_When_ConnectionUiIsRendered()
    {
        var component = Render<ReconnectModal>();

        component.Find("script").GetAttribute("src").ShouldEndWith("App/Layout/ReconnectModal.razor.js");
        component.Find("#components-reconnect-button").TextContent.Trim().ShouldBe("Повторить");
        component.Find("#components-resume-button").TextContent.Trim().ShouldBe("Продолжить");
        component.Find("#components-seconds-to-next-attempt").ShouldNotBeNull();
    }
}

