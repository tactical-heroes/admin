using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.Ui;

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

    protected bool IsSaving => IsSubmitting;

    protected Task SubmitAsync(
        Func<CancellationToken, Task<Result<Guid>>> saveAsync)
    {
        return base.SubmitAsync(
            cancellationToken => SaveAsync(saveAsync, cancellationToken));
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
        navigation.NavigateTo(successRoute(result.Value));
    }
}
