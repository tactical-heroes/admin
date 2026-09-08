using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace TacticalHeroes.Admin.Shared.Ui.Forms;

public abstract class MudCreateFormComponentBase<TModel, TValidator>(
    Func<TModel, CancellationToken, Task<Result<Guid>>> createAsync,
    string successMessage,
    Func<Guid, string> successRoute,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudFormComponentBase<TModel, TValidator>(
        snackbar,
        navigation,
        successMessage,
        successRoute)
    where TModel : class, new()
    where TValidator : MudFormValidator<TModel>, new()
{
    protected Task SubmitAsync()
    {
        return base.SubmitAsync(
            cancellationToken => createAsync(Model, cancellationToken));
    }
}
