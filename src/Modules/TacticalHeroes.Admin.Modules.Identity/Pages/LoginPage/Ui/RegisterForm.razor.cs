using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class RegisterForm(LoginApi loginApi)
{
    private bool _showPassword;
    private bool _showPasswordConfirmation;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private string LoginHref => IdentityRoutes.LoginPage(ReturnUrl);

    private string ConfirmationHref => IdentityRoutes.LoginPage(
        ReturnUrl,
        LoginMode.Confirmation);

    private Task SubmitAsync()
    {
        return SubmitResultAsync(cancellationToken =>
            loginApi.RegisterAsync(
                Model.Email,
                Model.UserName,
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
