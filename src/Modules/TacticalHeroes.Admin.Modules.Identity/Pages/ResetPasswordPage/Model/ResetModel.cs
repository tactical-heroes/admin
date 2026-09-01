namespace TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Model;

public sealed class ResetModel
{
    public string Password { get; set; } = string.Empty;

    public string PasswordConfirmation { get; set; } = string.Empty;
}
