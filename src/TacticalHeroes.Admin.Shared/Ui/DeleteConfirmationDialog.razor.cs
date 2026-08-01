using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class DeleteConfirmationDialog
{
    [CascadingParameter]
    private IMudDialogInstance Dialog { get; set; } = null!;

    [Parameter, EditorRequired]
    public string EntityType { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string EntityName { get; set; } = string.Empty;

    private void Cancel()
    {
        Dialog.Cancel();
    }

    private void Confirm()
    {
        Dialog.Close(DialogResult.Ok(true));
    }
}
