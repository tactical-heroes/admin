using Microsoft.AspNetCore.Components;

using MudBlazor;

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

    protected abstract Task SaveAsync();
}
