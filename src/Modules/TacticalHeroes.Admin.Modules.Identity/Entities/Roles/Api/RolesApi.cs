using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Shared.Model;

using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;

public sealed class RolesApi(TacticalHeroesApiClient client)
{
    public async Task<Result<PaginationResult<RoleListItem>>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Roles.GetAsync(
                request =>
                {
                    request.QueryParameters.PageNumber = pageNumber;
                    request.QueryParameters.PageSize = pageSize;
                },
                cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(response =>
        {
            if (response is null)
            {
                return PaginationResult<RoleListItem>.Empty(pageNumber, pageSize);
            }

            var items = response.Items?
                .Select(apiRole => new RoleListItem(
                    apiRole.Id!.Value,
                    apiRole.Name!))
                .ToArray() ?? [];

            return new PaginationResult<RoleListItem>(
                items,
                Math.Max(response.PageNumber ?? 0, pageNumber),
                Math.Max(response.PageSize ?? 0, pageSize),
                response.TotalCount ?? 0,
                checked((int)(response.TotalPages ?? 0)));
        });
    }

    public async Task<Result<RoleDetails>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Roles[id].GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(response => new RoleDetails
        {
            Id = response!.Id!.Value,
            Name = response.Name!,
            Claims = response.Claims!
                .Select(ToClaimValue)
                .ToList(),
        });
    }

    public async Task<Result<Guid>> CreateAsync(
        RoleDetails role,
        CancellationToken cancellationToken)
    {
        var request = new CreateRoleRequest
        {
            Name = role.Name.Trim(),
            Claims = role.Claims.Select(ToApiClaim).ToList(),
        };
        var result = await client.Api.V1.Roles.PostAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(response => response!.Id!.Value);
    }

    public async Task<Result> UpdateAsync(
        RoleDetails role,
        CancellationToken cancellationToken)
    {
        if (role.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                "A role identifier is required for update.");
        }

        var request = new UpdateRoleRequest
        {
            Name = role.Name.Trim(),
            Claims = role.Claims.Select(ToApiClaim).ToList(),
        };

        return await client.Api.V1.Roles[role.Id].PutAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }

    public Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Roles[id].DeleteAsync(
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
