using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Shared.Ui.Dialogs;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Dialogs;

public sealed class EntityDeleteButtonTests : BunitContext
{
    public EntityDeleteButtonTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory(DisplayName = "A delete button notifies its parent only after a successful confirmed deletion")]
    [InlineData(false, false, 0, 0)]
    [InlineData(true, false, 1, 1)]
    [InlineData(true, true, 1, 0)]
    public async Task ConfirmDeleteAsync_Should_RespectConfirmationAndResult_When_ActionCompletes(
        bool confirm, bool fail, int expectedDeletes, int expectedNotifications)
    {
        int deletes = 0;
        int notifications = 0;
        var dialogs = Render<MudDialogProvider>();
        var component = Render<EntityDeleteButton<int>>(parameters => parameters
            .Add(button => button.EntityId, 42)
            .Add(button => button.EntityType, "item")
            .Add(button => button.EntityName, "Example")
            .Add(button => button.DeleteLabel, "Delete")
            .Add(button => button.SuccessMessage, "Deleted")
            .Add(button => button.OnDeleted, () => notifications++)
            .Add(button => button.DeleteAsync, (id, cancellationToken) =>
            {
                id.ShouldBe(42);
                cancellationToken.IsCancellationRequested.ShouldBeFalse();
                deletes++;
                return Task.FromResult(fail ? Result.Failure(Error.Failure("Delete failed.")) : Result.Success());
            }));

        Task click = component.Find("button").ClickAsync(new MouseEventArgs());
        dialogs.WaitForAssertion(() => dialogs.Markup.ShouldContain("Example"));
        dialogs.FindAll("button").Single(button =>
            button.TextContent.Trim() == (confirm ? "Удалить" : "Отмена")).Click();
        await click;

        deletes.ShouldBe(expectedDeletes);
        notifications.ShouldBe(expectedNotifications);
        component.Find("button").HasAttribute("disabled").ShouldBeFalse();
        if (confirm)
        {
            Services.GetRequiredService<ISnackbar>().ShownSnackbars.Single().Message
                .ShouldBe(fail ? "Delete failed." : "Deleted");
        }
    }
}
