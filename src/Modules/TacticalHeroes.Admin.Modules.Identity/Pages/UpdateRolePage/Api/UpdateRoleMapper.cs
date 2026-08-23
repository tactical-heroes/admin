using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Model;

using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class UpdateRoleMapper
{
    [MapperIgnoreSource(nameof(GetRoleDetailsResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(GetRoleDetailsResponse.Id))]
    public static partial UpdateRoleFormModel ToForm(GetRoleDetailsResponse response);

    [MapperIgnoreTarget(nameof(UpdateRoleRequest.AdditionalData))]
    public static partial UpdateRoleRequest ToRequest(UpdateRoleFormModel role);

    [MapperIgnoreSource(nameof(ApiClaim.AdditionalData))]
    private static partial ClaimValue ToClaimValue(ApiClaim claim);

    private static List<ClaimValue> ToClaimValues(List<ApiClaim>? claims)
    {
        ArgumentNullException.ThrowIfNull(claims);
        return claims.Select(ToClaimValue).ToList();
    }

    [MapperIgnoreTarget(nameof(ApiClaim.AdditionalData))]
    private static partial ApiClaim ToApiClaim(ClaimValue claim);
}
