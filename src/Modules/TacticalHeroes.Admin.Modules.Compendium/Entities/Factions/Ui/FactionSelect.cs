using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Shared.Ui.Inputs;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Ui;

public sealed class FactionSelect(FactionOptionsApi factionOptionsApi)
    : AsyncSelectOption<Guid, FactionSelectOption>(
        factionOptionsApi.SearchAsync,
        "Фракция",
        "Фракции не найдены.",
        CompendiumRoutes.CreateFaction,
        "Создать фракцию");
