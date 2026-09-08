using TacticalHeroes.Admin.Client.App.Layout;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Layout;

public sealed class BlazorErrorUiTests : BunitContext
{
    [Fact(DisplayName = "Render should show recovery controls when error ui is rendered")]
    public void Render_Should_ShowRecoveryControls_When_ErrorUiIsRendered()
    {
        var component = Render<BlazorErrorUi>();

        component.Find("#blazor-error-ui .reload").GetAttribute("href").ShouldBe(".");
        component.Find("button.dismiss").GetAttribute("aria-label").ShouldBe("Закрыть уведомление");
    }
}
