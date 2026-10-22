namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;

public sealed class RoleOption : SelectOption<Guid>
{
    public bool IsDefault { get; set; }
}
