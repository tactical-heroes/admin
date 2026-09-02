using System.ComponentModel.DataAnnotations;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;

public enum LoginMode
{
    [Display(Name = "Register")]
    Register = 0,

    [Display(Name = "Confirm email")]
    Confirmation = 1,

    [Display(Name = "Recover access")]
    Recover = 2,
}
