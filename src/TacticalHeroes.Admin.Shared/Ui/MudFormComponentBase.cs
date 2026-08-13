using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.Ui;

public abstract class MudFormComponentBase<TModel, TValidator>(
    ISnackbar snackbar,
    NavigationManager navigation)
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

    protected ISnackbar Snackbar { get; } = snackbar;

    protected NavigationManager Navigation { get; } = navigation;

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
                await SaveAsync();
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected abstract Task<Result<Guid>> SaveCoreAsync();

    protected virtual void OnSaveSucceeded(Guid id)
    {
    }

    private async Task SaveAsync()
    {
        Errors.Clear();

        Result<Guid> result = await SaveCoreAsync();

        if (result.IsFailure)
        {
            Errors.Handle(result.Errors, Snackbar);
            return;
        }

        OnSaveSucceeded(result.Value);
    }
}
