using TacticalHeroes.Admin.Client.Entities.Claims.Model;

namespace TacticalHeroes.Admin.Client.Entities.Roles.Model;

public sealed class RoleDetails
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<ClaimValue> Claims { get; set; } = [];
}
