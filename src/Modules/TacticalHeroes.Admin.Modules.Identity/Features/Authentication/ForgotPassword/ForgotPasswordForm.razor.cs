using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Authentication.ForgotPassword;

public partial class ForgotPasswordForm
{
    private readonly EmailModel _model = new();
    private bool _submitting;
    private bool _requested;
    private string? _error;

    [Inject]
    private AuthenticationApi AuthenticationApi { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private string LoginHref => IdentityRoutes.LoginPage(ReturnUrl);

    private async Task SubmitAsync()
    {
        _submitting = true;
        _error = null;

        try
        {
            await AuthenticationApi.RequestPasswordResetAsync(_model.Email);
            _requested = true;
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

    private sealed class EmailModel
    {
        [Required(ErrorMessage = "Укажите электронную почту.")]
        [EmailAddress(ErrorMessage = "Укажите корректную электронную почту.")]
        public string Email { get; set; } = string.Empty;
    }
}
