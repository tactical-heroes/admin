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
    public async Task<Result<PaginationResult<UserListItem>>> GetPageAsync(
        int pageNumber,
        int pageSize,
        string? email,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Users.GetAsync(
                request =>
                {
                    request.QueryParameters.PageNumber = pageNumber;
                    request.QueryParameters.PageSize = pageSize;
                    request.QueryParameters.Email = string.IsNullOrWhiteSpace(email)
                        ? null
                        : email.Trim();
                },
                cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(response =>
        {
            if (response is null)
            {
                return PaginationResult<UserListItem>.Empty(pageNumber, pageSize);
            }

            var items = response.Items?
                .Select(apiUser => new UserListItem(
                    apiUser.Id!.Value,
                    apiUser.Email!,
                    apiUser.UserName!,
                    apiUser.IsConfirmed!.Value,
                    apiUser.Status!,
                    apiUser.StatusDisplayName!))
                .ToArray() ?? [];

            return new PaginationResult<UserListItem>(
                items,
                Math.Max(response.PageNumber ?? 0, pageNumber),
                Math.Max(response.PageSize ?? 0, pageSize),
                response.TotalCount ?? 0,
                checked((int)(response.TotalPages ?? 0)));
        });
    }

    public async Task<Result<UserDetails>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Users[id].GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(response => new UserDetails
        {
            Id = response!.Id!.Value,
            Email = response.Email!,
            UserName = response.UserName!,
            IsConfirmed = response.IsConfirmed!.Value,
            Status = response.Status!,
            StatusDisplayName = response.StatusDisplayName!,
            Claims = response.Claims!
                .Select(ToClaimValue)
                .ToList(),
        });
    }

    public async Task<Result<IReadOnlyList<UserStatus>>> GetStatusesAsync(
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Users.Statuses.GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(response =>
            (IReadOnlyList<UserStatus>)response!
                .Select(apiStatus => new UserStatus(
                    apiStatus.Name!,
                    apiStatus.DisplayName!))
                .ToArray());
    }

    public async Task<Result<Guid>> CreateAsync(
        UserDetails user,
        CancellationToken cancellationToken)
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
        var result = await client.Api.V1.Users.PostAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(response => response!.Id!.Value);
    }

    public async Task<Result> UpdateAsync(
        UserDetails user,
        CancellationToken cancellationToken)
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

        return await client.Api.V1.Users[user.Id].PutAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }

    public Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Users[id].DeleteAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }

    private static ClaimValue ToClaimValue(ApiClaim apiClaim)
    {
        return new ClaimValue
        {
            Type = apiClaim.Type!,
            Value = apiClaim.Value!,
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
