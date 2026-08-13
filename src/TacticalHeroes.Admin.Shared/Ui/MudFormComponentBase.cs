using MudBlazor;

namespace TacticalHeroes.Admin.Shared.Ui;

public abstract class MudFormComponentBase : CancelableComponentBase
{
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
