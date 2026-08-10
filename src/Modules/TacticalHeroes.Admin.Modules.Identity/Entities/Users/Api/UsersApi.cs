using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Shared.Model;

using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;

public sealed class UsersApi(TacticalHeroesApiClient client)
{
    public Task<Result<PaginationResult<UserListItem>>> GetPageAsync(
        int pageNumber,
        int pageSize,
        string? email,
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync(
            async () =>
            {
                var response = await client.Api.V1.Users.GetAsync(
                    request =>
                    {
                        request.QueryParameters.PageNumber = pageNumber;
                        request.QueryParameters.PageSize = pageSize;
                        request.QueryParameters.Email = string.IsNullOrWhiteSpace(email)
                            ? null
                            : email.Trim();
                    },
                    cancellationToken);

                if (response is null)
                {
                    return PaginationResult<UserListItem>.Empty(pageNumber, pageSize);
                }

                var items = response.Items?
                    .Where(user => user.Id.HasValue)
                    .Select(user => new UserListItem(
                        user.Id!.Value,
                        user.Email ?? string.Empty,
                        user.UserName ?? string.Empty,
                        user.IsConfirmed ?? false,
                        user.Status ?? string.Empty,
                        user.StatusDisplayName ?? user.Status ?? string.Empty))
                    .ToArray() ?? [];

                return new PaginationResult<UserListItem>(
                    items,
                    Math.Max(response.PageNumber ?? 0, pageNumber),
                    Math.Max(response.PageSize ?? 0, pageSize),
                    response.TotalCount ?? 0,
                    checked((int)(response.TotalPages ?? 0)));
            },
            cancellationToken);
    }

    public Task<Result<UserDetails>> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync(
            async () =>
            {
                var response = await client.Api.V1.Users[id].GetAsync(
                    cancellationToken: cancellationToken)
                    ?? throw new InvalidOperationException(
                        "The users API returned an empty response.");

                return new UserDetails
                {
                    Id = response.Id ?? id,
                    Email = response.Email ?? string.Empty,
                    UserName = response.UserName ?? string.Empty,
                    IsConfirmed = response.IsConfirmed ?? false,
                    Status = response.Status ?? string.Empty,
                    StatusDisplayName = response.StatusDisplayName ?? response.Status ?? string.Empty,
                    Claims = response.Claims?
                        .Select(ToClaimValue)
                        .ToList() ?? [],
                };
            },
            cancellationToken);
    }

    public Task<Result<IReadOnlyList<UserStatus>>> GetStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync<IReadOnlyList<UserStatus>>(
            async () =>
            {
                var response = await client.Api.V1.Users.Statuses.GetAsync(
                    cancellationToken: cancellationToken);

                return response?
                    .Where(status => !string.IsNullOrWhiteSpace(status.Name))
                    .Select(status => new UserStatus(
                        status.Name!,
                        status.DisplayName ?? status.Name!))
                    .ToArray() ?? [];
            },
            cancellationToken);
    }

    public Task<Result<Guid>> CreateAsync(
        UserDetails user,
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync(
            async () =>
            {
                var request = new CreateUserRequest
                {
                    Email = user.Email.Trim(),
                    UserName = user.UserName.Trim(),
                    Password = user.Password,
                    IsConfirmed = user.IsConfirmed,
                    Status = user.Status,
                    Claims = user.Claims.Select(ToApiClaim).ToList(),
                };
                var response = await client.Api.V1.Users.PostAsync(
                    request,
                    cancellationToken: cancellationToken)
                    ?? throw new InvalidOperationException(
                        "The users API returned an empty response.");

                return response.Id
                    ?? throw new InvalidOperationException(
                        "The users API did not return the created identifier.");
            },
            cancellationToken);
    }

    public Task<Result> UpdateAsync(
        UserDetails user,
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync(
            async () =>
            {
                if (user.Id == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "A user identifier is required for update.");
                }

                var request = new UpdateUserRequest
                {
                    Email = user.Email.Trim(),
                    UserName = user.UserName.Trim(),
                    IsConfirmed = user.IsConfirmed,
                    Status = user.Status,
                    Claims = user.Claims.Select(ToApiClaim).ToList(),
                };

                await client.Api.V1.Users[user.Id].PutAsync(
                    request,
                    cancellationToken: cancellationToken);
            },
            cancellationToken);
    }

    public Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync(
            () => client.Api.V1.Users[id].DeleteAsync(
                cancellationToken: cancellationToken),
            cancellationToken);
    }

    private static ClaimValue ToClaimValue(ApiClaim claim)
    {
        return new ClaimValue
        {
            Type = claim.Type ?? string.Empty,
            Value = claim.Value ?? string.Empty,
        };
    }

    private static ApiClaim ToApiClaim(ClaimValue claim)
    {
        return new ApiClaim
        {
            Type = claim.Type.Trim(),
            Value = claim.Value.Trim(),
        };
    }
}
