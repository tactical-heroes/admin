using MudBlazor.Services;

using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class EditFormActionsTests : BunitContext
{
    public EditFormActionsTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory(DisplayName = "Shows the action matching the editing mode")]
    [InlineData(true, "Создать")]
    [InlineData(false, "Сохранить")]
    public void Render_Should_ShowExpectedAction_When_ModeIsProvided(
        bool isNew,
        string expectedAction)
    {
        var component = Render<EditFormActions>(parameters => parameters
            .Add(actions => actions.CancelHref, "/items")
            .Add(actions => actions.IsNew, isNew)
            .Add(actions => actions.OnSubmit, () => { }));

        component.Find(".submit-action").TextContent.ShouldContain(expectedAction);
        component.Find("a").TextContent.ShouldContain("Отмена");
    }
}
