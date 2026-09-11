using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Api;

public sealed class UnitListApi(TacticalHeroesApiClient client)
{
    public async Task<Result<PaginationResult<UnitListItem>>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Units.GetAsync(
                request =>
                {
                    request.QueryParameters.PageNumber = pageNumber;
                    request.QueryParameters.PageSize = pageSize;
                },
                cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UnitListMapper.ToPage);
    }

    public Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Units[id].DeleteAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }
}
