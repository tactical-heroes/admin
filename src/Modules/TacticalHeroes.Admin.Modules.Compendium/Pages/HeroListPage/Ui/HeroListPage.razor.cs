using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Compendium.Pages.HeroListPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.HeroListPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.HeroListPage.Ui;

public partial class HeroListPage(
    HeroListApi heroListApi,
    NavigationManager navigation)
    : MudPagedListComponentBase<HeroListItem>(
        heroListApi.GetPageAsync,
        CompendiumRoutes.Heroes,
        navigation)
{
    private Task<Result> DeleteHeroAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return heroListApi.DeleteAsync(id, cancellationToken);
    }
}
