using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.Authentication;

public partial class ConfirmEmailPage
{
    [SupplyParameterFromQuery(Name = "userId")]
    public Guid? UserId { get; set; }

    [SupplyParameterFromQuery(Name = "emailConfirmationToken")]
    public string? EmailConfirmationToken { get; set; }
}
