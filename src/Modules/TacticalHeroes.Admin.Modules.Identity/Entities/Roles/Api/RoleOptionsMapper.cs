using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class RoleOptionsMapper
{
    [MapperIgnoreSource(nameof(RoleListItemResponse.AdditionalData))]
    private static partial RoleOption ToOption(RoleListItemResponse response);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(
        nameof(PaginationResultOfRoleListItemResponse.Items),
        nameof(PaginationResult<RoleOption>.Items),
        SuppressNullMismatchDiagnostic = true)]
    public static partial PaginationResult<RoleOption> ToPage(PaginationResultOfRoleListItemResponse response);
}
