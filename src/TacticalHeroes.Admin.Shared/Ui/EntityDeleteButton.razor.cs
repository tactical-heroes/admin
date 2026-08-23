using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class EntityDeleteButton<TKey>(
    IDialogService dialogService,
    ISnackbar snackbar)
    : CancelableComponentBase
{
    [Parameter, EditorRequired]
    public string EntityType { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string EntityName { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string DeleteLabel { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string SuccessMessage { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public TKey EntityId { get; set; } = default!;

    [Parameter, EditorRequired]
    public Func<TKey, CancellationToken, Task<Result>> DeleteAsync { get; set; } = null!;

    [Parameter]
    public EventCallback OnDeleted { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    private bool IsDeleting { get; set; }

    private async Task ConfirmDeleteAsync()
    {
        if (IsDeleting ||
            !await dialogService.ConfirmDeleteAsync(EntityType, EntityName))
        {
            return;
        }

        IsDeleting = true;

        try
        {
            Result result = await DeleteAsync(EntityId, LifetimeToken);

            if (result.IsFailure)
            {
                snackbar.Add(ApiErrorMessage.FromErrors(result.Errors), Severity.Error);
                return;
            }

            snackbar.Add(SuccessMessage, Severity.Success);
            await OnDeleted.InvokeAsync();
        }
        finally
        {
            IsDeleting = false;
        }
    }
}
