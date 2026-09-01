using System.ComponentModel.DataAnnotations;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Model;

public sealed class EmailModel
{
    [Required(ErrorMessage = "Укажите email.")]
    [EmailAddress(ErrorMessage = "Укажите корректный email.")]
    public string Email { get; set; } = string.Empty;
}
