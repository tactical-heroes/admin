using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;

using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
internal static partial class UpdateUserMapper
{
    [MapperIgnoreSource(nameof(GetUserDetailsResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(GetUserDetailsResponse.Id))]
    [MapperIgnoreSource(nameof(GetUserDetailsResponse.StatusDisplayName))]
    public static partial UpdateUserFormModel ToForm(GetUserDetailsResponse response);

    [MapperIgnoreTarget(nameof(UpdateUserRequest.AdditionalData))]
    public static partial UpdateUserRequest ToRequest(UpdateUserFormModel user);

    [MapperIgnoreSource(nameof(UserStatusResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(UserStatusResponse.Id))]
    private static partial UserStatus ToStatus(UserStatusResponse response);

    [MapperIgnoreSource(nameof(ApiClaim.AdditionalData))]
    private static partial ClaimValue ToClaimValue(ApiClaim claim);

    private static List<ClaimValue> ToClaimValues(List<ApiClaim>? claims)
    {
        ArgumentNullException.ThrowIfNull(claims);
        return claims.Select(ToClaimValue).ToList();
    }

    [MapperIgnoreTarget(nameof(ApiClaim.AdditionalData))]
    private static partial ApiClaim ToApiClaim(ClaimValue claim);

    [MapperIgnore]
    public static IReadOnlyList<UserStatus> ToStatuses(
        IReadOnlyCollection<UserStatusResponse> response)
    {
        return response.Select(ToStatus).ToArray();
    }
}
