using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;

public sealed class UserDetails
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public bool IsConfirmed { get; set; }

    public string Status { get; set; } = string.Empty;

    public string StatusDisplayName { get; set; } = string.Empty;

    public List<ClaimValue> Claims { get; set; } = [];
}
