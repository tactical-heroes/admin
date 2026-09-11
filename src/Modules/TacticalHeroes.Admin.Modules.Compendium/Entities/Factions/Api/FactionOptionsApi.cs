using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;

public sealed class FactionOptionsApi(TacticalHeroesApiClient client)
{
    public async Task<Result<IReadOnlyList<FactionSelectOption>>> SearchAsync(
        string? search,
        int limit,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Factions.SelectOptions.GetAsync(
                request =>
                {
                    request.QueryParameters.Search = search;
                    request.QueryParameters.Limit = limit;
                },
                cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(FactionOptionsMapper.ToOptions);
    }
}
