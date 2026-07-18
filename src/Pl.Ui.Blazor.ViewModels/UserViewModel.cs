using Common.Enums;
using Common.Helpers;

using Pl.Ui.Blazor.ViewModels.Core;

using System.ComponentModel.DataAnnotations;

namespace Pl.Ui.Blazor.ViewModels;

public sealed record UserViewModel : BaseViewModel<Guid>
{
    [Display(Name = "Роль")]
    public Role? Role { get; set; }

    [Display(Name = "Имя")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Телефон")]
    public string Phone
    {
        get => PhoneHelper.ToUiRuPhone(_phone);
        set => _phone = PhoneHelper.ToServerRuPhone(value);
    }

    [Display(Name = "Возраст")]
    public int Age
    {
        get
        {
            if (Birthday is null)
            {
                return 0;
            }

            var today = DateTime.Today;
            var age = today.Year - Birthday.Value.Year;

            if (Birthday.Value.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }

    [Display(Name = "День рождения")]
    public DateTime? Birthday { get; set; }

    private string _phone = string.Empty;

    [Display(Name = "Аватар")]
    public string? Avatar { get; set; }
}
