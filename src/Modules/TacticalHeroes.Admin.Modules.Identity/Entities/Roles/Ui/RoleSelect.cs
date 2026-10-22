using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Shared.Ui.Inputs;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Ui;

public sealed class RoleSelect(RoleOptionsApi api)
    : AsyncSelectOption<Guid, RoleOption>(
        api.GetAllAsync,
        "Роли пользователя",
        "Роли не найдены.",
        "Можно назначить несколько ролей или оставить пользователя без ролей.")
{
    protected override string GetOptionName(RoleOption option) =>
        option.IsDefault ? $"{option.Name} (по умолчанию)" : option.Name;
}
