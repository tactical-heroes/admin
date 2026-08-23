using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Api;

public sealed class FactionListApi(TacticalHeroesApiClient client)
{
    public async Task<Result<PaginationResult<FactionListItem>>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Factions.GetAsync(
                request =>
                {
                    request.QueryParameters.PageNumber = pageNumber;
                    request.QueryParameters.PageSize = pageSize;
                },
                cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(FactionListMapper.ToPage);
    }

    public Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Factions[id].DeleteAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }
}
