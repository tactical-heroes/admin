using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Components;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Api;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Ui;

public partial class ResetPasswordPage
{
    private readonly ResetModel _model = new();
    private bool _submitting;
    private bool _completed;
    private bool _showPassword;
    private bool _showPasswordConfirmation;
    private string? _error;

    [Inject]
    private AuthApi AuthApi { get; set; } = null!;

    [Parameter]
    public Guid? UserId { get; set; }

    [Parameter]
    public string? PasswordResetToken { get; set; }

    private async Task SubmitAsync()
    {
        if (!UserId.HasValue || string.IsNullOrWhiteSpace(PasswordResetToken))
        {
            return;
        }

        _submitting = true;
        _error = null;

        Result result = await AuthApi.ResetPasswordAsync(
            UserId.Value,
            PasswordResetToken,
            _model.Password,
            LifetimeToken);

        if (result.IsFailure)
        {
            _error = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            _completed = true;
        }

        _submitting = false;
    }

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }

    private void TogglePasswordConfirmationVisibility()
    {
        _showPasswordConfirmation = !_showPasswordConfirmation;
    }

    private sealed class ResetModel
    {
        [Required(ErrorMessage = "Укажите новый пароль.")]
        [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторите новый пароль.")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
        public string PasswordConfirmation { get; set; } = string.Empty;
    }
}
