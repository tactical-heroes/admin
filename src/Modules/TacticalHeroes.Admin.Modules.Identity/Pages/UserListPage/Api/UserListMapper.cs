using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Api;

[Mapper]
internal static partial class UserListMapper
{
    [MapperIgnoreSource(nameof(UserListItemResponse.AdditionalData))]
    private static partial UserListItem ToListItem(UserListItemResponse response);

    [MapperIgnore]
    public static PaginationResult<UserListItem> ToPage(
        PaginationResultOfUserListItemResponse response,
        int pageNumber,
        int pageSize)
    {
        UserListItem[] items = response.Items?
            .Select(ToListItem)
            .ToArray() ?? [];

        return new PaginationResult<UserListItem>(
            items,
            Math.Max(response.PageNumber ?? 0, pageNumber),
            Math.Max(response.PageSize ?? 0, pageSize),
            response.TotalCount ?? 0,
            checked((int)(response.TotalPages ?? 0)));
    }
}
