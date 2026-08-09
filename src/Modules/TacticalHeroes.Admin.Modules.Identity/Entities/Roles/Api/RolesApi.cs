using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Shared.Model;

using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;

public sealed class RolesApi(TacticalHeroesApiClient client)
{
    public async Task<PaginationResult<RoleSummary>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await client.Api.V1.Roles.GetAsync(
            request =>
            {
                request.QueryParameters.PageNumber = pageNumber;
                request.QueryParameters.PageSize = pageSize;
            },
            cancellationToken);

        if (response is null)
        {
            return PaginationResult<RoleSummary>.Empty(pageNumber, pageSize);
        }

        var items = response.Items?
            .Where(role => role.Id.HasValue)
            .Select(role => new RoleSummary(
                role.Id!.Value,
                role.Name ?? string.Empty))
            .ToArray() ?? [];

        return new PaginationResult<RoleSummary>(
            items,
            Math.Max(response.PageNumber ?? 0, pageNumber),
            Math.Max(response.PageSize ?? 0, pageSize),
            response.TotalCount ?? 0,
            checked((int)(response.TotalPages ?? 0)));
    }

    public async Task<RoleDetails> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await client.Api.V1.Roles[id].GetAsync(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The roles API returned an empty response.");

        return new RoleDetails
        {
            Id = response.Id ?? id,
            Name = response.Name ?? string.Empty,
            Claims = response.Claims?
                .Select(ToClaimValue)
                .ToList() ?? [],
        };
    }

    public async Task<Guid> CreateAsync(
        RoleDetails role,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateRoleRequest
        {
            Name = role.Name.Trim(),
            Claims = role.Claims.Select(ToApiClaim).ToList(),
        };
        var response = await client.Api.V1.Roles.PostAsync(
            request,
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The roles API returned an empty response.");

        return response.Id
            ?? throw new InvalidOperationException("The roles API did not return the created identifier.");
    }

    public Task UpdateAsync(
        RoleDetails role,
        CancellationToken cancellationToken = default)
    {
        if (role.Id == Guid.Empty)
        {
            throw new InvalidOperationException("A role identifier is required for update.");
        }

        var request = new UpdateRoleRequest
        {
            Name = role.Name.Trim(),
            Claims = role.Claims.Select(ToApiClaim).ToList(),
        };

        return client.Api.V1.Roles[role.Id].PutAsync(
            request,
            cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return client.Api.V1.Roles[id].DeleteAsync(
            cancellationToken: cancellationToken);
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
