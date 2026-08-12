using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;

public sealed class FactionsApi(TacticalHeroesApiClient client)
{
    public Task<Result<PaginationResult<FactionListItem>>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync(
            async () =>
            {
                var response = await client.Api.V1.Factions.GetAsync(
                    request =>
                    {
                        request.QueryParameters.PageNumber = pageNumber;
                        request.QueryParameters.PageSize = pageSize;
                    },
                    cancellationToken);

                if (response is null)
                {
                    return PaginationResult<FactionListItem>.Empty(pageNumber, pageSize);
                }

                var items = response.Items?
                    .Select(apiFaction => new FactionListItem(
                        apiFaction.Id!.Value,
                        apiFaction.Name!,
                        apiFaction.Description!))
                    .ToArray() ?? [];

                return new PaginationResult<FactionListItem>(
                    items,
                    Math.Max(response.PageNumber ?? 0, pageNumber),
                    Math.Max(response.PageSize ?? 0, pageSize),
                    response.TotalCount ?? 0,
                    checked((int)(response.TotalPages ?? 0)));
            },
            cancellationToken);
    }

    public Task<Result<FactionDetails>> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync(
            async () =>
            {
                var response = await client.Api.V1.Factions[id].GetAsync(
                    cancellationToken: cancellationToken);

                return new FactionDetails
                {
                    Id = response!.Id!.Value,
                    Name = response.Name!,
                    Description = response.Description!,
                };
            },
            cancellationToken);
    }

    public Task<Result<Guid>> CreateAsync(
        FactionDetails faction,
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync(
            async () =>
            {
                var request = new CreateFactionRequest
                {
                    Name = faction.Name.Trim(),
                    Description = faction.Description.Trim(),
                };
                var response = await client.Api.V1.Factions.PostAsync(
                    request,
                    cancellationToken: cancellationToken);

                return response!.Id!.Value;
            },
            cancellationToken);
    }

    public Task<Result> UpdateAsync(
        FactionDetails faction,
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync(
            async () =>
            {
                if (!faction.Id.HasValue)
                {
                    throw new InvalidOperationException(
                        "A faction identifier is required for update.");
                }

                var request = new UpdateFactionRequest
                {
                    Name = faction.Name.Trim(),
                    Description = faction.Description.Trim(),
                };

                await client.Api.V1.Factions[faction.Id.Value].PutAsync(
                    request,
                    cancellationToken: cancellationToken);
            },
            cancellationToken);
    }

    public Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ApiResult.ExecuteAsync(
            () => client.Api.V1.Factions[id].DeleteAsync(
                cancellationToken: cancellationToken),
            cancellationToken);
    }
}
