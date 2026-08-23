using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Model;

public sealed class CreateUserFormModel
{
    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool IsConfirmed { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<ClaimValue> Claims { get; set; } = [];
}
