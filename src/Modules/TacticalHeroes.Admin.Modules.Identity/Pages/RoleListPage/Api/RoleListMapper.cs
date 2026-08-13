using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Api;

[Mapper]
internal static partial class RoleListMapper
{
    [MapperIgnoreSource(nameof(RoleListItemResponse.AdditionalData))]
    private static partial RoleListItem ToListItem(RoleListItemResponse response);

    [MapperIgnore]
    public static PaginationResult<RoleListItem> ToPage(
        PaginationResultOfRoleListItemResponse response,
        int pageNumber,
        int pageSize)
    {
        RoleListItem[] items = response.Items?
            .Select(ToListItem)
            .ToArray() ?? [];

        return new PaginationResult<RoleListItem>(
            items,
            Math.Max(response.PageNumber ?? 0, pageNumber),
            Math.Max(response.PageSize ?? 0, pageSize),
            response.TotalCount ?? 0,
            checked((int)(response.TotalPages ?? 0)));
    }
}
