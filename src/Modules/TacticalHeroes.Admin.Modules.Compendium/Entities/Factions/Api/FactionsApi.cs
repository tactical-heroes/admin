using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;

public sealed class FactionsApi(TacticalHeroesApiClient client)
{
    public async Task<PaginationResult<FactionSummary>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
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
            return PaginationResult<FactionSummary>.Empty(pageNumber, pageSize);
        }

        var items = response.Items?
            .Where(faction => faction.Id.HasValue)
            .Select(faction => new FactionSummary(
                faction.Id!.Value,
                faction.Name ?? string.Empty,
                faction.Description ?? string.Empty))
            .ToArray() ?? [];

        return new PaginationResult<FactionSummary>(
            items,
            Math.Max(response.PageNumber ?? 0, pageNumber),
            Math.Max(response.PageSize ?? 0, pageSize),
            response.TotalCount ?? 0,
            checked((int)(response.TotalPages ?? 0)));
    }

    public async Task<FactionDetails> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await client.Api.V1.Factions[id].GetAsync(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The factions API returned an empty response.");

        return new FactionDetails
        {
            Id = response.Id ?? id,
            Name = response.Name ?? string.Empty,
            Description = response.Description ?? string.Empty,
        };
    }

    public async Task<Guid> CreateAsync(
        FactionDetails faction,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateFactionRequest
        {
            Name = faction.Name.Trim(),
            Description = faction.Description.Trim(),
        };
        var response = await client.Api.V1.Factions.PostAsync(
            request,
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The factions API returned an empty response.");

        return response.Id
            ?? throw new InvalidOperationException("The factions API did not return the created identifier.");
    }

    public Task UpdateAsync(
        FactionDetails faction,
        CancellationToken cancellationToken = default)
    {
        if (!faction.Id.HasValue)
        {
            throw new InvalidOperationException("A faction identifier is required for update.");
        }

        var request = new UpdateFactionRequest
        {
            Name = faction.Name.Trim(),
            Description = faction.Description.Trim(),
        };

        return client.Api.V1.Factions[faction.Id.Value].PutAsync(
            request,
            cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return client.Api.V1.Factions[id].DeleteAsync(
            cancellationToken: cancellationToken);
    }
}
