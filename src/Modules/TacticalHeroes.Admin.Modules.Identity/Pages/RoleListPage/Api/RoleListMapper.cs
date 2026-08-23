using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class RoleListMapper
{
    [MapperIgnoreSource(nameof(RoleListItemResponse.AdditionalData))]
    private static partial RoleListItem ToListItem(RoleListItemResponse response);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(
        nameof(PaginationResultOfRoleListItemResponse.Items),
        nameof(PaginationResult<RoleListItem>.Items),
        SuppressNullMismatchDiagnostic = true)]
    public static partial PaginationResult<RoleListItem> ToPage(
        PaginationResultOfRoleListItemResponse response);
}
