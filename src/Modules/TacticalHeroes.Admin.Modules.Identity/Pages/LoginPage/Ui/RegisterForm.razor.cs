using Microsoft.AspNetCore.Components;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class RegisterForm(LoginApi loginApi)
{
    private readonly RegisterModel _model = new();
    private bool _submitting;
    private bool _registered;
    private bool _showPassword;
    private bool _showPasswordConfirmation;
    private string? _error;

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

        Result<Guid> result = await loginApi.RegisterAsync(
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
}
