using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;

public sealed class UserStatus : IEnumeration
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}
