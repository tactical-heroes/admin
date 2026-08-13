using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Components;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class RegisterForm
{
    private readonly RegisterModel _model = new();
    private bool _submitting;
    private bool _registered;
    private bool _showPassword;
    private bool _showPasswordConfirmation;
    private string? _error;

    [Inject]
    private AuthApi AuthApi { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private string LoginHref => IdentityRoutes.LoginPage(ReturnUrl);

    private string ConfirmationHref => IdentityRoutes.LoginPage(
        ReturnUrl,
        LoginMode.Confirmation);

    private async Task SubmitAsync()
    {
        _submitting = true;
        _error = null;

        Result<Guid> result = await AuthApi.RegisterAsync(
            _model.Email,
            _model.UserName,
            _model.Password,
            LifetimeToken);

        if (result.IsFailure)
        {
            _error = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            _registered = true;
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

    private sealed class RegisterModel
    {
        [Required(ErrorMessage = "Укажите email.")]
        [EmailAddress(ErrorMessage = "Укажите корректный email.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите имя пользователя.")]
        [MinLength(2, ErrorMessage = "Имя пользователя слишком короткое.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите пароль.")]
        [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторите пароль.")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
        public string PasswordConfirmation { get; set; } = string.Empty;
    }
}
