using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class UnitListMapper
{
    [MapperIgnoreSource(nameof(UnitListItemResponse.AdditionalData))]
    public static partial UnitListItem ToListItem(UnitListItemResponse response);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(
        nameof(PaginationResultOfUnitListItemResponse.Items),
        nameof(PaginationResult<UnitListItem>.Items),
        SuppressNullMismatchDiagnostic = true)]
    public static partial PaginationResult<UnitListItem> ToPage(
        PaginationResultOfUnitListItemResponse response);
}
