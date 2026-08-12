using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.Auth;

public partial class LoginPage
{
    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery(Name = "error")]
    public string? Error { get; set; }

    [SupplyParameterFromQuery(Name = "mode")]
    public string? Mode { get; set; }

    private string NormalizedMode => Mode?.ToLowerInvariant() switch
    {
        "register" => "register",
        "confirmation" => "confirmation",
        "recover" => "recover",
        _ => "login",
    };

    private string Title => NormalizedMode switch
    {
        "register" => "Регистрация · Tactical Heroes",
        "confirmation" => "Подтверждение email · Tactical Heroes",
        "recover" => "Восстановление доступа · Tactical Heroes",
        _ => "Вход · Tactical Heroes",
    };
}
