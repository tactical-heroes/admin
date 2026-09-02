using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class AsyncActionButtonTests : BunitContext
{
    public AsyncActionButtonTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Shows busy state and disables the action")]
    public void Render_Should_ShowBusyState_When_ActionIsBusy()
    {
        var component = Render<AsyncActionButton>(parameters => parameters
            .Add(button => button.Text, "Save")
            .Add(button => button.BusyText, "Saving…")
            .Add(button => button.Busy, true)
            .Add(button => button.OnClick, () => { }));

        AngleSharp.Dom.IElement button = component.Find("button");
        button.HasAttribute("disabled").ShouldBeTrue();
        button.GetAttribute("aria-busy").ShouldBe("true");
        button.TextContent.ShouldContain("Saving…");
        component.FindComponent<MudProgressCircular>();
    }

    [Fact(DisplayName = "Invokes the action when the button is clicked")]
    public void Click_Should_InvokeAction_When_ButtonIsEnabled()
    {
        int clickCount = 0;
        var component = Render<AsyncActionButton>(parameters => parameters
            .Add(button => button.Text, "Save")
            .Add(button => button.BusyText, "Saving…")
            .Add(button => button.Class, "custom-action")
            .Add(button => button.OnClick, () => clickCount++));

        component.Find("button.custom-action").Click();

        clickCount.ShouldBe(1);
    }
}
