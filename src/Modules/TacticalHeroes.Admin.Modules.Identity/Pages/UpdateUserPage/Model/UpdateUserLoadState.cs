using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;

public sealed record UpdateUserLoadState(
    UpdateUserFormModel User,
    IReadOnlyList<UserStatus> Statuses);
