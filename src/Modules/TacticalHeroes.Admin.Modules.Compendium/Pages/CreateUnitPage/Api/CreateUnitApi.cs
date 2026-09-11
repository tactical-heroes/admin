using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Api;

public sealed class CreateUnitApi(TacticalHeroesApiClient client)
{
    public async Task<Result<Guid>> CreateAsync(
        CreateUnitFormModel unit,
        CancellationToken cancellationToken)
    {
        var request = CreateUnitMapper.ToRequest(unit);
        if (request.Shots is null)
        {
            request.AdditionalData["shots"] = null!;
        }

        if (request.RangedAttackRange is null)
        {
            request.AdditionalData["rangedAttackRange"] = null!;
        }

        var result = await client.Api.V1.Units.PostAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(CreateUnitMapper.ToId);
    }
}
