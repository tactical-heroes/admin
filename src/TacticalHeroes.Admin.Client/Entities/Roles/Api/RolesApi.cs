using System.Globalization;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Client.Entities.Claims.Model;
using TacticalHeroes.Admin.Client.Entities.Roles.Model;
using TacticalHeroes.Admin.Client.Shared.Api;
using TacticalHeroes.Admin.Client.Shared.Model;
using ApiClaim = TacticalHeroes.Admin.Api.Generated.Models.Claim;

namespace TacticalHeroes.Admin.Client.Entities.Roles.Api;

public sealed class RolesApi(TacticalHeroesApiClient client)
{
    public async Task<PageResult<RoleSummary>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await client.Api.V1.Roles.GetAsync(
            request =>
            {
                request.QueryParameters.PageNumber = pageNumber.ToString(
                    CultureInfo.InvariantCulture);
                request.QueryParameters.PageSize = pageSize.ToString(
                    CultureInfo.InvariantCulture);
            },
            cancellationToken);

        if (response is null)
        {
            return PageResult<RoleSummary>.Empty(pageNumber, pageSize);
        }

        var items = response.Items?
            .Where(role => role.Id.HasValue)
            .Select(role => new RoleSummary(
                role.Id!.Value,
                role.Name ?? string.Empty))
            .ToArray() ?? [];

        return new PageResult<RoleSummary>(
            items,
            checked((int)Math.Max(response.PageNumber.ToInt64(), pageNumber)),
            checked((int)Math.Max(response.PageSize.ToInt64(), pageSize)),
            response.TotalCount.ToInt64(),
            checked((int)response.TotalPages.ToInt64()));
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

    public Task UpdateAsync(
        RoleDetails role,
        CancellationToken cancellationToken = default)
    {
        var request = new UpdateRoleRequest
        {
            Name = role.Name.Trim(),
            Claims = role.Claims.Select(ToApiClaim).ToList(),
        };

        return client.Api.V1.Roles[role.Id].PutAsync(
            request,
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
