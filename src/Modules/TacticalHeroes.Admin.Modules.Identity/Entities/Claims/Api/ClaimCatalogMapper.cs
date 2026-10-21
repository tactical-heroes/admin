using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class ClaimCatalogMapper
{
    [MapperIgnoreSource(nameof(GetPermissionsResponse.AdditionalData))]
    [MapProperty(
        nameof(GetPermissionsResponse.Items),
        nameof(ClaimCatalog.Items),
        Use = nameof(ToItems))]
    public static partial ClaimCatalog ToPermissions(GetPermissionsResponse response);

    [MapperIgnoreSource(nameof(PermissionResponse.AdditionalData))]
    private static partial ClaimOption ToPermission(PermissionResponse response);

    public static IReadOnlyList<ClaimOption> ToItems(List<PermissionResponse>? items) =>
        RequiredValueMapper.ToRequiredList(items, ToPermission);
}
