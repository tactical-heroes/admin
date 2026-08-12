using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Shared.Model;

using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;

[Mapper(ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class RolesMapper
{
    [MapperIgnoreSource(nameof(RoleListItemResponse.AdditionalData))]
    public static partial RoleListItem ToListItem(RoleListItemResponse response);

    [MapperIgnoreSource(nameof(GetRoleDetailsResponse.AdditionalData))]
    public static partial RoleDetails ToDetails(GetRoleDetailsResponse response);

    [MapperIgnoreSource(nameof(RoleDetails.Id))]
    [MapperIgnoreTarget(nameof(CreateRoleRequest.AdditionalData))]
    [MapProperty(nameof(RoleDetails.Name), nameof(CreateRoleRequest.Name), Use = nameof(Trim))]
    public static partial CreateRoleRequest ToCreateRequest(RoleDetails role);

    [MapperIgnoreSource(nameof(RoleDetails.Id))]
    [MapperIgnoreTarget(nameof(UpdateRoleRequest.AdditionalData))]
    [MapProperty(nameof(RoleDetails.Name), nameof(UpdateRoleRequest.Name), Use = nameof(Trim))]
    public static partial UpdateRoleRequest ToUpdateRequest(RoleDetails role);

    [MapperIgnoreSource(nameof(ApiClaim.AdditionalData))]
    private static partial ClaimValue ToClaimValue(ApiClaim claim);

    [MapperIgnoreTarget(nameof(ApiClaim.AdditionalData))]
    [MapProperty(nameof(ClaimValue.Type), nameof(ApiClaim.Type), Use = nameof(Trim))]
    [MapProperty(nameof(ClaimValue.Value), nameof(ApiClaim.Value), Use = nameof(Trim))]
    private static partial ApiClaim ToApiClaim(ClaimValue claim);

    [MapperIgnore]
    public static PaginationResult<RoleListItem> ToPage(
        PaginationResultOfRoleListItemResponse response,
        int pageNumber,
        int pageSize)
    {
        var items = response.Items?
            .Select(ToListItem)
            .ToArray() ?? [];

        return new PaginationResult<RoleListItem>(
            items,
            Math.Max(response.PageNumber ?? 0, pageNumber),
            Math.Max(response.PageSize ?? 0, pageSize),
            response.TotalCount ?? 0,
            checked((int)(response.TotalPages ?? 0)));
    }

    [MapperIgnore]
    public static Guid ToId(CreateRoleResponse response)
    {
        return response.Id
            ?? throw new ArgumentNullException(nameof(response.Id));
    }

    [UserMapping(Default = false)]
    private static string Trim(string value)
    {
        return value.Trim();
    }
}
