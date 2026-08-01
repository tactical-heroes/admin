using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Authentication.ResetPassword;

public partial class ResetPasswordForm
{
    private readonly ResetModel _model = new();
    private bool _submitting;
    private bool _completed;
    private bool _showPassword;
    private bool _showPasswordConfirmation;
    private string? _error;

    [Inject]
    private AuthenticationApi AuthenticationApi { get; set; } = null!;

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

        try
        {
            await AuthenticationApi.ResetPasswordAsync(
                UserId.Value,
                PasswordResetToken,
                _model.Password);
            _completed = true;
        }
        catch (Exception exception)
        {
            _error = ApiErrorMessage.FromException(exception);
        }
        finally
        {
            _submitting = false;
        }
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
