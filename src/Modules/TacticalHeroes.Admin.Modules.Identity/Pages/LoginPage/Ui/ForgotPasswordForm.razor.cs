using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Components;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class ForgotPasswordForm
{
    private readonly EmailModel _model = new();
    private bool _submitting;
    private bool _requested;
    private string? _error;

    [Inject]
    private LoginApi LoginApi { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private string LoginHref => IdentityRoutes.LoginPage(ReturnUrl);

    private async Task SubmitAsync()
    {
        _submitting = true;
        _error = null;

        Result result = await LoginApi.RequestPasswordResetAsync(
            _model.Email,
            LifetimeToken);

        if (result.IsFailure)
        {
            _error = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            _requested = true;
        }

        _submitting = false;
    }

    private sealed class EmailModel
    {
        [Required(ErrorMessage = "Укажите email.")]
        [EmailAddress(ErrorMessage = "Укажите корректный email.")]
        public string Email { get; set; } = string.Empty;
    }
}
