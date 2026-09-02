using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;
using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class LoginForm
{
    private bool _showPassword;

    [Parameter]
    public string? ReturnUrl { get; set; }

    [Parameter]
    public AuthenticationError? Error { get; set; }

    private string? ErrorMessage => Error?.GetDisplayName();

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }

    private string BuildModeHref(LoginMode mode)
    {
        return IdentityRoutes.LoginPage(ReturnUrl, mode);
    }
}
