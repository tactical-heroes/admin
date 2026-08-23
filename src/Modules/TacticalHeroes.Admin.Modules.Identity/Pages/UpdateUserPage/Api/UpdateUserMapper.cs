using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api;

[Mapper]
[UseStaticMapper(typeof(RequiredValueMapper))]
[UseStaticMapper(typeof(ClaimMapper))]
internal static partial class UpdateUserMapper
{
    [MapperIgnoreSource(nameof(GetUserDetailsResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(GetUserDetailsResponse.Id))]
    [MapperIgnoreSource(nameof(GetUserDetailsResponse.StatusDisplayName))]
    public static partial UpdateUserFormModel ToForm(GetUserDetailsResponse response);

    [MapperIgnoreTarget(nameof(UpdateUserRequest.AdditionalData))]
    public static partial UpdateUserRequest ToRequest(UpdateUserFormModel user);
}
