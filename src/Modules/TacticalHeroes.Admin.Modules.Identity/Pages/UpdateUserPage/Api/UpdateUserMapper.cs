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
    [MapProperty(
        nameof(GetUserDetailsResponse.Claims),
        nameof(UpdateUserFormModel.Claims),
        Use = nameof(@ClaimMapper.ToValues))]
    [MapProperty(
        nameof(GetUserDetailsResponse.RoleIds),
        nameof(UpdateUserFormModel.RoleIds),
        Use = nameof(ToRoleIds))]
    public static partial UpdateUserFormModel ToForm(GetUserDetailsResponse response);

    [MapperIgnoreTarget(nameof(UpdateUserRequest.AdditionalData))]
    [MapProperty(
        nameof(UpdateUserFormModel.Claims),
        nameof(UpdateUserRequest.Claims),
        Use = nameof(@ClaimMapper.ToApiValues))]
    [MapProperty(
        nameof(UpdateUserFormModel.RoleIds),
        nameof(UpdateUserRequest.RoleIds),
        Use = nameof(ToApiRoleIds))]
    public static partial UpdateUserRequest ToRequest(UpdateUserFormModel user);

    private static List<Guid> ToRoleIds(List<Guid?>? roleIds) =>
        RequiredValueMapper.ToRequiredList(roleIds, RequiredValueMapper.ToRequiredGuid);

    private static List<Guid?> ToApiRoleIds(List<Guid>? roleIds) =>
        RequiredValueMapper.ToRequiredList(roleIds, static id => (Guid?)id);
}
