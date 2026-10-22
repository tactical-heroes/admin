using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Model;

public sealed class UpdateRoleFormModel
{
    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public List<ClaimValue> Claims { get; set; } = [];
}
