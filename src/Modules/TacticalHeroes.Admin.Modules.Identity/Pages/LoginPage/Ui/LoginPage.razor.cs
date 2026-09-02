using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;
using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class LoginPage
{
    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery(Name = "error")]
    public string? Error { get; set; }

    [SupplyParameterFromQuery(Name = "mode")]
    public string? Mode { get; set; }

    private LoginMode? SelectedMode => Mode.TryParseSnakeCase(out LoginMode mode)
        ? mode
        : null;

    private AuthenticationError? ParsedError =>
        Error.TryParseSnakeCase(out AuthenticationError error)
            ? error
            : null;

    private string Title => SelectedMode switch
    {
        LoginMode.Register => "Регистрация · Tactical Heroes",
        LoginMode.Confirmation => "Подтверждение email · Tactical Heroes",
        LoginMode.Recover => "Восстановление доступа · Tactical Heroes",
        _ => "Вход · Tactical Heroes",
    };
}
