using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class UserListMapper
{
    [MapperIgnoreSource(nameof(UserListItemResponse.AdditionalData))]
    private static partial UserListItem ToListItem(UserListItemResponse response);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(
        nameof(PaginationResultOfUserListItemResponse.Items),
        nameof(PaginationResult<UserListItem>.Items),
        Use = nameof(ToItems))]
    public static partial PaginationResult<UserListItem> ToPage(
        PaginationResultOfUserListItemResponse response);

    public static IReadOnlyList<UserListItem> ToItems(List<UserListItemResponse>? items) =>
        RequiredValueMapper.ToRequiredList(items, ToListItem);
}
