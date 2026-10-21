using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class ClaimMapper
{
    [MapperIgnoreSource(nameof(ApiClaim.AdditionalData))]
    public static partial ClaimValue ToValue(ApiClaim claim);

    public static List<ClaimValue> ToValues(List<ApiClaim>? claims) =>
        RequiredValueMapper.ToRequiredList(claims, ToValue);

    [MapperIgnoreTarget(nameof(ApiClaim.AdditionalData))]
    public static partial ApiClaim ToApi(ClaimValue claim);

    public static List<ApiClaim> ToApiValues(List<ClaimValue>? claims) =>
        RequiredValueMapper.ToRequiredList(claims, ToApi);
}
