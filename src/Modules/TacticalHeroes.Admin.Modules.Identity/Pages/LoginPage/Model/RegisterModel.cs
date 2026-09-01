using System.ComponentModel.DataAnnotations;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Model;

public sealed class RegisterModel
{
    [Required(ErrorMessage = "Укажите email.")]
    [EmailAddress(ErrorMessage = "Укажите корректный email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите имя пользователя.")]
    [MinLength(2, ErrorMessage = "Имя пользователя слишком короткое.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль.")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторите пароль.")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
    public string PasswordConfirmation { get; set; } = string.Empty;
}
