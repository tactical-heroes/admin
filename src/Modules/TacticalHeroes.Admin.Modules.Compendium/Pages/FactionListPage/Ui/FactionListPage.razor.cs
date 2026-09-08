using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Ui;

public partial class FactionListPage(
    FactionListApi factionListApi,
    NavigationManager navigation)
    : MudPagedListComponentBase<FactionListItem>(
        factionListApi.GetPageAsync,
        CompendiumRoutes.Factions,
        navigation)
{
    private Task<Result> DeleteFactionAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return factionListApi.DeleteAsync(id, cancellationToken);
    }
}
