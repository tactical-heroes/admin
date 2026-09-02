using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Ui;

public partial class ResetPasswordPage(ResetPasswordApi resetPasswordApi)
{
    private bool _showPassword;
    private bool _showPasswordConfirmation;

    [Parameter]
    public Guid? UserId { get; set; }

    [Parameter]
    public string? PasswordResetToken { get; set; }

    private Task SubmitAsync()
    {
        if (!UserId.HasValue ||
            string.IsNullOrWhiteSpace(PasswordResetToken))
        {
            return Task.CompletedTask;
        }

        return SubmitResultAsync(cancellationToken =>
            resetPasswordApi.ResetPasswordAsync(
                UserId.Value,
                PasswordResetToken,
                Model.Password,
                cancellationToken));
    }

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }

    private void TogglePasswordConfirmationVisibility()
    {
        _showPasswordConfirmation = !_showPasswordConfirmation;
    }
}
