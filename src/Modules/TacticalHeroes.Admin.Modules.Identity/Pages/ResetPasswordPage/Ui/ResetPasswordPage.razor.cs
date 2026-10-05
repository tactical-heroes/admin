using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Ui;

public partial class ResetPasswordPage(ResetPasswordApi resetPasswordApi)
{
    private bool _showPassword;
    private bool _showPasswordConfirmation;

    [SupplyParameterFromQuery]
    public string? UserId { get; set; }

    [SupplyParameterFromQuery]
    public string? PasswordResetToken { get; set; }

    private Guid? ParsedUserId =>
        Guid.TryParse(UserId, out var userId) && userId != Guid.Empty ? userId : null;

    private Task SubmitAsync()
    {
        var userId = ParsedUserId;
        if (!userId.HasValue ||
            string.IsNullOrWhiteSpace(PasswordResetToken))
        {
            return Task.CompletedTask;
        }

        return SubmitResultAsync(cancellationToken =>
            resetPasswordApi.ResetPasswordAsync(
                userId.Value,
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
