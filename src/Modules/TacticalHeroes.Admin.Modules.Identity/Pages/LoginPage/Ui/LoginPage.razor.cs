using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class LoginPage
{
    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery]
    public string? Error { get; set; }

    [SupplyParameterFromQuery]
    public string? Mode { get; set; }

    private LoginMode? SelectedMode => Mode.TryParseSnakeCase(out LoginMode mode)
        ? mode
        : null;

    private AuthenticationError? ParsedError =>
        Error.TryParseSnakeCase(out AuthenticationError error)
            ? error
            : null;

    private string Title => $"{SelectedMode?.GetDisplayName() ?? "Sign in"} - {Branding.GameName}";
}
