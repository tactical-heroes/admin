using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Model;

using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;

[Mapper]
internal static partial class CreateRoleMapper
{
    [MapperIgnoreTarget(nameof(CreateRoleRequest.AdditionalData))]
    public static partial CreateRoleRequest ToRequest(CreateRoleFormModel role);

    [MapperIgnoreTarget(nameof(ApiClaim.AdditionalData))]
    private static partial ApiClaim ToApiClaim(ClaimValue claim);

    [MapperIgnore]
    public static Guid ToId(CreateRoleResponse response)
    {
        return response.Id!.Value;
    }
}
