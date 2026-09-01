using System.ComponentModel.DataAnnotations;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Model;

public sealed class ResetModel
{
    [Required(ErrorMessage = "Укажите новый пароль.")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторите новый пароль.")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
    public string PasswordConfirmation { get; set; } = string.Empty;
}
