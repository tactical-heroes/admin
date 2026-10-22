using System.Security.Claims;

using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Forms;

public sealed class EditFormActionsTests : BunitContext
{
    public EditFormActionsTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory(DisplayName = "Render should require permission for submission when permission is configured")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Render_Should_RequirePermissionForSubmission_When_PermissionIsConfigured(bool isNew, bool hasPermission)
    {
        string permission = isNew ? "items.create" : "items.update";
        var authorization = AddAuthorization();
        authorization.SetAuthorized("User");
        authorization.SetClaims(new Claim("permission", hasPermission ? permission : "items.read"));
        int submissions = 0;

        var component = Render<EditFormActions>(parameters => parameters
            .Add(actions => actions.CancelHref, "/items")
            .Add(actions => actions.IsNew, isNew)
            .Add(actions => actions.Permission, permission)
            .Add(actions => actions.OnSubmit, () => submissions++));

        component.Find("a").GetAttribute("href").ShouldBe("/items");
        if (hasPermission)
        {
            component.Find(".submit-action").TextContent.ShouldContain(isNew ? "Создать" : "Сохранить");
            component.Find(".submit-action").Click();
            submissions.ShouldBe(1);
        }
        else
        {
            component.FindAll(".submit-action").ShouldBeEmpty();
            submissions.ShouldBe(0);
        }
    }

    [Theory(DisplayName = "Render should show expected action when mode is provided")]
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
