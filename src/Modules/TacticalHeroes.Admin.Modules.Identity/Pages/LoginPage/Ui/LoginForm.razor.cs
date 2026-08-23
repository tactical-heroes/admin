using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class LoginForm
{
    private bool _showPassword;

    [Parameter]
    public string? ReturnUrl { get; set; }

    [Parameter]
    public string? Error { get; set; }

    private string? ErrorMessage => Error switch
    {
        "invalid_credentials" => "Неверный email или пароль.",
        "forbidden" => "Аккаунт не подтверждён, заблокирован или временно заблокирован после неудачных попыток.",
        "invalid_request" => "Ссылка входа устарела или повреждена. Начните вход заново.",
        "unavailable" => "Сервис авторизации временно недоступен. Повторите попытку позже.",
        "oauth" => "OAuth-вход не удалось завершить. Начните его заново.",
        _ => null,
    };

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }

    private string BuildModeHref(LoginMode mode)
    {
        return IdentityRoutes.LoginPage(ReturnUrl, mode);
    }
}
