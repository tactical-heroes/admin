using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace TacticalHeroes.Admin.Shared.Ui.Forms;

public abstract class MudFormComponentBase<TModel, TValidator>(
    ISnackbar snackbar,
    NavigationManager navigation,
    string successMessage,
    Func<Guid, string> successRoute)
    : MudValidatedFormComponentBase<TModel, TValidator>
    where TModel : class, new()
    where TValidator : MudFormValidator<TModel>, new()
{
    [PersistentState(AllowUpdates = true)]
    public override TModel Model
    {
        get => base.Model;
        set => base.Model = value;
    }

    protected FormErrorState<TModel> Errors { get; } = new();

    protected NavigationManager Navigation { get; } = navigation;

    protected bool IsSaving => IsSubmitting;

    protected Task SubmitAsync(
        Func<CancellationToken, Task<Result<Guid>>> saveAsync)
    {
        return base.SubmitAsync(
            cancellationToken => SaveAsync(saveAsync, cancellationToken));
    }

    protected virtual Task NavigateAfterSaveAsync(Guid id)
    {
        Navigation.NavigateTo(successRoute(id));
        return Task.CompletedTask;
    }

    private async Task SaveAsync(
        Func<CancellationToken, Task<Result<Guid>>> saveAsync,
        CancellationToken cancellationToken)
    {
        Errors.Clear();

        Result<Guid> result = await saveAsync(cancellationToken);

        if (result.IsFailure)
        {
            Errors.Handle(result.Errors, snackbar);
            return;
        }

        snackbar.Add(successMessage, Severity.Success);
        await NavigateAfterSaveAsync(result.Value);
    }
}
