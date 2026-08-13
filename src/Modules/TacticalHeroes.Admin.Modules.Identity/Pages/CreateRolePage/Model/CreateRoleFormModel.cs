using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Model;

public sealed class CreateRoleFormModel
{
    public string Name { get; set; } = string.Empty;

    public List<ClaimValue> Claims { get; set; } = [];
}
