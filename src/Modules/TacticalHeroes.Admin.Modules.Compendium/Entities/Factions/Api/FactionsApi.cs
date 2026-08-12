using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;

public sealed class FactionsApi(TacticalHeroesApiClient client)
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

        return result.Map(response => FactionsMapper.ToPage(response, pageNumber, pageSize));
    }

    public async Task<Result<FactionDetails>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Factions[id].GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(FactionsMapper.ToDetails);
    }

    public async Task<Result<Guid>> CreateAsync(
        FactionDetails faction,
        CancellationToken cancellationToken)
    {
        var request = FactionsMapper.ToCreateRequest(faction);
        var result = await client.Api.V1.Factions.PostAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(FactionsMapper.ToId);
    }

    public async Task<Result> UpdateAsync(
        FactionDetails faction,
        CancellationToken cancellationToken)
    {
        if (!faction.Id.HasValue)
        {
            throw new InvalidOperationException(
                "A faction identifier is required for update.");
        }

        var request = FactionsMapper.ToUpdateRequest(faction);

        return await client.Api.V1.Factions[faction.Id.Value].PutAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
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
