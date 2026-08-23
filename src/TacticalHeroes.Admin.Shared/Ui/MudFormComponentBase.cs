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
    : CancelableComponentBase
    where TModel : class, new()
    where TValidator : MudFormValidator<TModel>, new()
{
    private TModel? _model;

    [PersistentState(AllowUpdates = true)]
    public TModel Model
    {
        get => _model ??= new();
        set => _model = value;
    }

    protected FormErrorState<TModel> Errors { get; } = new();

    protected TValidator Validator { get; } = new();

    protected MudForm? Form { get; set; }

    protected bool IsValid { get; set; }

    protected bool IsSaving { get; private set; }

    protected async Task SubmitAsync(
        Func<CancellationToken, Task<Result<Guid>>> saveAsync)
    {
        if (Form is null || IsSaving)
        {
            return;
        }

        IsSaving = true;

        try
        {
            await Form.ValidateAsync();

            if (IsValid)
            {
                await SaveAsync(saveAsync);
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task SaveAsync(
        Func<CancellationToken, Task<Result<Guid>>> saveAsync)
    {
        Errors.Clear();

        Result<Guid> result = await saveAsync(LifetimeToken);

        if (result.IsFailure)
        {
            Errors.Handle(result.Errors, snackbar);
            return;
        }

        snackbar.Add(successMessage, Severity.Success);
        navigation.NavigateTo(successRoute(result.Value));
    }
}
