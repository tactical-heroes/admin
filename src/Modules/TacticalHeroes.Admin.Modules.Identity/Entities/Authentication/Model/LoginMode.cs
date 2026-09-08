using System.ComponentModel.DataAnnotations;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;

public enum LoginMode
{
    [Display(Name = "Register - Tactical Heroes")]
    Register = 0,

    [Display(Name = "Confirm email - Tactical Heroes")]
    Confirmation = 1,

    [Display(Name = "Recover access - Tactical Heroes")]
    Recover = 2,
}
