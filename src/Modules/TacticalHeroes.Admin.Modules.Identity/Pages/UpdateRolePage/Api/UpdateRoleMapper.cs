using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
[UseStaticMapper(typeof(ClaimMapper))]
internal static partial class UpdateRoleMapper
{
    [MapperIgnoreSource(nameof(GetRoleDetailsResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(GetRoleDetailsResponse.Id))]
    public static partial UpdateRoleFormModel ToForm(GetRoleDetailsResponse response);

    [MapperIgnoreTarget(nameof(UpdateRoleRequest.AdditionalData))]
    public static partial UpdateRoleRequest ToRequest(UpdateRoleFormModel role);
}
