using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;

[Mapper]
[UseStaticMapper(typeof(ClaimMapper))]
internal static partial class CreateRoleMapper
{
    [MapperIgnoreTarget(nameof(CreateRoleRequest.AdditionalData))]
    public static partial CreateRoleRequest ToRequest(CreateRoleFormModel role);

    [MapperIgnore]
    public static Guid ToId(CreateRoleResponse response)
    {
        return response.Id!.Value;
    }
}
