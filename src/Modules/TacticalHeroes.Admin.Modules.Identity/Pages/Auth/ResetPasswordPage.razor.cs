using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.Auth;

public partial class ResetPasswordPage
{
    [SupplyParameterFromQuery(Name = "userId")]
    public Guid? UserId { get; set; }

    [SupplyParameterFromQuery(Name = "passwordResetToken")]
    public string? PasswordResetToken { get; set; }
}
