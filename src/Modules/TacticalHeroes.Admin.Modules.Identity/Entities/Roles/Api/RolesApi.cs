using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Shared.Model;

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

        return result.Map(response => RolesMapper.ToPage(response, pageNumber, pageSize));
    }

    public async Task<Result<RoleDetails>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Roles[id].GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(RolesMapper.ToDetails);
    }

    public async Task<Result<Guid>> CreateAsync(
        RoleDetails role,
        CancellationToken cancellationToken)
    {
        var request = RolesMapper.ToCreateRequest(role);
        var result = await client.Api.V1.Roles.PostAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(RolesMapper.ToId);
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

        var request = RolesMapper.ToUpdateRequest(role);

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
}
