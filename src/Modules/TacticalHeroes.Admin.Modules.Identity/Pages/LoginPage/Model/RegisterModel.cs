namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Model;

public sealed class RegisterModel
{
    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string PasswordConfirmation { get; set; } = string.Empty;
}
