using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Api;

[Mapper]
internal static partial class FactionListMapper
{
    [MapperIgnoreSource(nameof(FactionListItemResponse.AdditionalData))]
    public static partial FactionListItem ToListItem(FactionListItemResponse response);

    [MapperIgnore]
    public static PaginationResult<FactionListItem> ToPage(
        PaginationResultOfFactionListItemResponse response,
        int pageNumber,
        int pageSize)
    {
        var items = response.Items?
            .Select(ToListItem)
            .ToArray() ?? [];

        return new PaginationResult<FactionListItem>(
            items,
            Math.Max(response.PageNumber ?? 0, pageNumber),
            Math.Max(response.PageSize ?? 0, pageSize),
            response.TotalCount ?? 0,
            checked((int)(response.TotalPages ?? 0)));
    }
}
