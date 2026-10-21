using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class FactionListMapper
{
    [MapperIgnoreSource(nameof(FactionListItemResponse.AdditionalData))]
    public static partial FactionListItem ToListItem(FactionListItemResponse response);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(
        nameof(PaginationResultOfFactionListItemResponse.Items),
        nameof(PaginationResult<FactionListItem>.Items),
        Use = nameof(ToItems))]
    public static partial PaginationResult<FactionListItem> ToPage(
        PaginationResultOfFactionListItemResponse response);

    public static IReadOnlyList<FactionListItem> ToItems(List<FactionListItemResponse>? items) =>
        RequiredValueMapper.ToRequiredList(items, ToListItem);
}
