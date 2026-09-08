using System.ComponentModel.DataAnnotations;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;

public enum LoginMode
{
    [Display(Name = "Регистрация · Tactical Heroes")]
    Register = 0,

    [Display(Name = "Подтверждение email · Tactical Heroes")]
    Confirmation = 1,

    [Display(Name = "Восстановление доступа · Tactical Heroes")]
    Recover = 2,
}
