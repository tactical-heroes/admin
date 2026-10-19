namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;

public sealed class RoleOption
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}
