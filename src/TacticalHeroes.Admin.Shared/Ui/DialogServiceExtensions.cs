using MudBlazor;

namespace TacticalHeroes.Admin.Shared.Ui;

public static class DialogServiceExtensions
{
    public static async Task<bool> ConfirmDeleteAsync(
        this IDialogService dialogService,
        string entityType,
        string entityName)
    {
        var parameters = new DialogParameters
        {
            [nameof(DeleteConfirmationDialog.EntityType)] = entityType,
            [nameof(DeleteConfirmationDialog.EntityName)] = entityName,
        };
        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraSmall,
        };
        IDialogReference dialog = await dialogService.ShowAsync<DeleteConfirmationDialog>(
            string.Empty,
            parameters,
            options);
        DialogResult? result = await dialog.Result;

        return result is { Canceled: false };
    }
}
