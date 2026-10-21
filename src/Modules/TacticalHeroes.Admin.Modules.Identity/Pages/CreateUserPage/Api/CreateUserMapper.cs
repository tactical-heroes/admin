using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Api;

[Mapper]
[UseStaticMapper(typeof(ClaimMapper))]
internal static partial class CreateUserMapper
{
    [MapperIgnoreTarget(nameof(CreateUserRequest.AdditionalData))]
    [MapProperty(
        nameof(CreateUserFormModel.Claims),
        nameof(CreateUserRequest.Claims),
        Use = nameof(@ClaimMapper.ToApiValues))]
    [MapProperty(
        nameof(CreateUserFormModel.RoleIds),
        nameof(CreateUserRequest.RoleIds),
        Use = nameof(ToApiRoleIds))]
    public static partial CreateUserRequest ToRequest(CreateUserFormModel user);

    [MapperIgnore]
    public static Guid ToId(CreateUserResponse response)
    {
        return response.Id!.Value;
    }

    private static List<Guid?> ToApiRoleIds(List<Guid>? roleIds) =>
        RequiredValueMapper.ToRequiredList(roleIds, static id => (Guid?)id);
}
