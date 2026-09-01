using FluentValidation.Results;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class ResendConfirmationForm(LoginApi loginApi)
{
    private readonly EmailModel _model = new();
    private readonly EmailModelValidator _validator = new();
    private MudForm? _form;
    private bool _isValid;
    private bool _submitting;
    private bool _requested;
    private string? _error;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private string LoginHref => IdentityRoutes.LoginPage(ReturnUrl);

    private async Task SubmitAsync()
    {
        if (_form is null || _submitting)
        {
            return;
        }

        await _form.ValidateAsync();
        ValidationResult validationResult = await _validator.ValidateAsync(
            _model,
            LifetimeToken);

        if (!_isValid || !validationResult.IsValid)
        {
            return;
        }

        _submitting = true;
        _error = null;

        Result result = await loginApi.ResendConfirmationEmailAsync(
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
