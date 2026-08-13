using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Api;

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

        return result.Map(response => RoleListMapper.ToPage(response, pageNumber, pageSize));
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
