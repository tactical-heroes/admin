using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;

public sealed class UserStatus(string name, string displayName) : IEnumeration
{
    public string Name { get; } = name;

    public string DisplayName { get; } = displayName;
}
