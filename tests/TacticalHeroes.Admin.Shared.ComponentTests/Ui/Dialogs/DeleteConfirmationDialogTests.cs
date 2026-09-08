using Microsoft.Extensions.DependencyInjection;

using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Shared.Ui.Dialogs;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Dialogs;

public sealed class DeleteConfirmationDialogTests : BunitContext
{
    public DeleteConfirmationDialogTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Confirming deletion returns a positive dialog result")]
    public async Task Confirm_Should_ReturnPositiveResult_When_DeleteIsClicked()
    {
        var (provider, dialog) = await ShowDialogAsync();

        provider.FindAll("button").Single(button => button.TextContent.Trim() == "Удалить").Click();
        DialogResult? result = await dialog.Result;

        result.ShouldNotBeNull();
        result.Canceled.ShouldBeFalse();
        result.Data.ShouldBe(true);
    }

    [Fact(DisplayName = "Cancelling deletion returns a cancelled dialog result")]
    public async Task Cancel_Should_ReturnCancelledResult_When_CancelIsClicked()
    {
        var (provider, dialog) = await ShowDialogAsync();

        provider.FindAll("button").Single(button => button.TextContent.Trim() == "Отмена").Click();
        DialogResult? result = await dialog.Result;

        result.ShouldNotBeNull();
        result.Canceled.ShouldBeTrue();
    }

    private async Task<(IRenderedComponent<MudDialogProvider>, IDialogReference)> ShowDialogAsync()
    {
        var provider = Render<MudDialogProvider>();
        var service = Services.GetRequiredService<IDialogService>();
        var parameters = new DialogParameters<DeleteConfirmationDialog>
        {
            { dialog => dialog.EntityType, "фракцию" },
            { dialog => dialog.EntityName, "Northern Alliance" },
        };
        IDialogReference dialog = await provider.InvokeAsync(() =>
            service.ShowAsync<DeleteConfirmationDialog>(string.Empty, parameters));
        provider.WaitForAssertion(() => provider.Markup.ShouldContain("Northern Alliance"));
        return (provider, dialog);
    }
}
