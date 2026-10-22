using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Compendium.Pages.HeroListPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.HeroListPage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class HeroListMapper
{
    [MapperIgnoreSource(nameof(HeroListItemResponse.AdditionalData))]
    public static partial HeroListItem ToListItem(HeroListItemResponse response);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(
        nameof(PaginationResultOfHeroListItemResponse.Items),
        nameof(PaginationResult<HeroListItem>.Items),
        SuppressNullMismatchDiagnostic = true)]
    public static partial PaginationResult<HeroListItem> ToPage(
        PaginationResultOfHeroListItemResponse response);
}
