using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.Ui;

public abstract class MudFormComponentBase<TModel, TValidator> : CancelableComponentBase
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

    [Inject]
    protected ISnackbar Snackbar { get; set; } = null!;

    protected MudForm? Form { get; set; }

    protected bool IsValid { get; set; }

    protected bool IsSaving { get; private set; }

    protected async Task SubmitAsync()
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
                await ExecuteSaveAsync();
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected async Task SaveAsync(
        Func<Task<Result>> saveAsync,
        Action onSuccess)
    {
        Errors.Clear();

        Result result = await saveAsync();

        if (result.IsFailure)
        {
            Errors.Handle(result.Errors, Snackbar);
            return;
        }

        onSuccess();
    }

    protected async Task SaveAsync<T>(
        Func<Task<Result<T>>> saveAsync,
        Action<T> onSuccess)
    {
        Errors.Clear();

        Result<T> result = await saveAsync();

        if (result.IsFailure)
        {
            Errors.Handle(result.Errors, Snackbar);
            return;
        }

        onSuccess(result.Value);
    }

    protected abstract Task ExecuteSaveAsync();
}
