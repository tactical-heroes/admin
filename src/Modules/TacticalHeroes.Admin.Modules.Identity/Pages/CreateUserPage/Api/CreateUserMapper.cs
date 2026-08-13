using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Model;

using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Api;

[Mapper]
internal static partial class CreateUserMapper
{
    [MapperIgnoreTarget(nameof(CreateUserRequest.AdditionalData))]
    public static partial CreateUserRequest ToRequest(CreateUserFormModel user);

    [MapperIgnoreSource(nameof(UserStatusResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(UserStatusResponse.Id))]
    private static partial UserStatus ToStatus(UserStatusResponse response);

    [MapperIgnoreTarget(nameof(ApiClaim.AdditionalData))]
    private static partial ApiClaim ToApiClaim(ClaimValue claim);

    [MapperIgnore]
    public static IReadOnlyList<UserStatus> ToStatuses(
        IReadOnlyCollection<UserStatusResponse> response)
    {
        return response.Select(ToStatus).ToArray();
    }

    [MapperIgnore]
    public static Guid ToId(CreateUserResponse response)
    {
        return response.Id!.Value;
    }
}
