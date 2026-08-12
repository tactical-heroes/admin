using Riok.Mapperly.Abstractions;

using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Shared.Model;

using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;

[Mapper(ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class UsersMapper
{
    [MapperIgnoreSource(nameof(UserListItemResponse.AdditionalData))]
    public static partial UserListItem ToListItem(UserListItemResponse response);

    [MapperIgnoreSource(nameof(GetUserDetailsResponse.AdditionalData))]
    [MapperIgnoreTarget(nameof(UserDetails.Password))]
    public static partial UserDetails ToDetails(GetUserDetailsResponse response);

    [MapperIgnoreSource(nameof(UserStatusResponse.AdditionalData))]
    [MapperIgnoreSource(nameof(UserStatusResponse.Id))]
    public static partial UserStatus ToStatus(UserStatusResponse response);

    [MapperIgnoreSource(nameof(UserDetails.Id))]
    [MapperIgnoreSource(nameof(UserDetails.StatusDisplayName))]
    [MapperIgnoreTarget(nameof(CreateUserRequest.AdditionalData))]
    [MapProperty(nameof(UserDetails.Email), nameof(CreateUserRequest.Email), Use = nameof(Trim))]
    [MapProperty(nameof(UserDetails.UserName), nameof(CreateUserRequest.UserName), Use = nameof(Trim))]
    public static partial CreateUserRequest ToCreateRequest(UserDetails user);

    [MapperIgnoreSource(nameof(UserDetails.Id))]
    [MapperIgnoreSource(nameof(UserDetails.Password))]
    [MapperIgnoreSource(nameof(UserDetails.StatusDisplayName))]
    [MapperIgnoreTarget(nameof(UpdateUserRequest.AdditionalData))]
    [MapProperty(nameof(UserDetails.Email), nameof(UpdateUserRequest.Email), Use = nameof(Trim))]
    [MapProperty(nameof(UserDetails.UserName), nameof(UpdateUserRequest.UserName), Use = nameof(Trim))]
    public static partial UpdateUserRequest ToUpdateRequest(UserDetails user);

    [MapperIgnoreSource(nameof(ApiClaim.AdditionalData))]
    private static partial ClaimValue ToClaimValue(ApiClaim claim);

    [MapperIgnoreTarget(nameof(ApiClaim.AdditionalData))]
    [MapProperty(nameof(ClaimValue.Type), nameof(ApiClaim.Type), Use = nameof(Trim))]
    [MapProperty(nameof(ClaimValue.Value), nameof(ApiClaim.Value), Use = nameof(Trim))]
    private static partial ApiClaim ToApiClaim(ClaimValue claim);

    [MapperIgnore]
    public static PaginationResult<UserListItem> ToPage(
        PaginationResultOfUserListItemResponse response,
        int pageNumber,
        int pageSize)
    {
        var items = response.Items?
            .Select(ToListItem)
            .ToArray() ?? [];

        return new PaginationResult<UserListItem>(
            items,
            Math.Max(response.PageNumber ?? 0, pageNumber),
            Math.Max(response.PageSize ?? 0, pageSize),
            response.TotalCount ?? 0,
            checked((int)(response.TotalPages ?? 0)));
    }

    [MapperIgnore]
    public static IReadOnlyList<UserStatus> ToStatuses(
        IReadOnlyCollection<UserStatusResponse> response)
    {
        return response.Select(ToStatus).ToArray();
    }

    [MapperIgnore]
    public static Guid ToId(CreateUserResponse response)
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
