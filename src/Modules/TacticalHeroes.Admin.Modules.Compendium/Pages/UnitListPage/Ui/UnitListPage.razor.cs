using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Ui;

public partial class UnitListPage(
    UnitListApi unitListApi,
    NavigationManager navigation)
    : MudPagedListComponentBase<UnitListItem>(
        unitListApi.GetPageAsync,
        CompendiumRoutes.Units,
        navigation)
{
    private Task<Result> DeleteUnitAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return unitListApi.DeleteAsync(id, cancellationToken);
    }
}
