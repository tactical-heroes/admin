using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Mapping;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;

public sealed class FactionOptionsApi(TacticalHeroesApiClient client)
{
    public async Task<Result<IReadOnlyList<FactionOption>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        List<FactionOption> options = [];

        // ponytail: the API only supports pagination; use server search if the faction catalog grows.
        for (int pageNumber = 1; ; pageNumber++)
        {
            var result = await client.Api.V1.Factions.GetAsync(
                    request =>
                    {
                        request.QueryParameters.PageNumber = pageNumber;
                        request.QueryParameters.PageSize = 100;
                    },
                    cancellationToken)
                .ToApiResultAsync(cancellationToken);

            if (result.IsFailure)
            {
                return Result.Failure<IReadOnlyList<FactionOption>>(result.Errors);
            }

            var page = result.Value;
            options.AddRange((page.Items ?? []).Select(faction => new FactionOption
            {
                Id = RequiredValueMapper.ToRequiredGuid(faction.Id),
                Name = RequiredValueMapper.ToRequiredString(faction.Name)
            }));

            if (pageNumber >= RequiredValueMapper.ToRequiredInt32(page.TotalPages))
            {
                return Result.Success<IReadOnlyList<FactionOption>>(options);
            }
        }
    }
}
