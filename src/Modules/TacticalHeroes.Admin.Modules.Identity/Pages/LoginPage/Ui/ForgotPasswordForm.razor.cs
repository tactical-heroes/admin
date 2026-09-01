using Microsoft.AspNetCore.Components;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class ForgotPasswordForm(LoginApi loginApi)
{
    private readonly EmailModel _model = new();
    private readonly EmailModelValidator _validator = new();
    private bool _submitting;
    private bool _requested;
    private string? _error;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private string LoginHref => IdentityRoutes.LoginPage(ReturnUrl);

    private async Task SubmitAsync()
    {
        _submitting = true;
        _error = null;

        Result result = await loginApi.RequestPasswordResetAsync(
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
}
