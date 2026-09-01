using FluentValidation.Results;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Ui;

public partial class ResetPasswordPage(ResetPasswordApi resetPasswordApi)
{
    private readonly ResetModel _model = new();
    private readonly ResetModelValidator _validator = new();
    private MudForm? _form;
    private bool _isValid;
    private bool _submitting;
    private bool _completed;
    private bool _showPassword;
    private bool _showPasswordConfirmation;
    private string? _error;

    [Parameter]
    public Guid? UserId { get; set; }

    [Parameter]
    public string? PasswordResetToken { get; set; }

    private async Task SubmitAsync()
    {
        if (!UserId.HasValue ||
            string.IsNullOrWhiteSpace(PasswordResetToken) ||
            _form is null ||
            _submitting)
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

        Result result = await resetPasswordApi.ResetPasswordAsync(
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
}
