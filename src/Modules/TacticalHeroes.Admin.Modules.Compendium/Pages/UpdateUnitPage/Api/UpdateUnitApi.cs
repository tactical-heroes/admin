using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Api;

public sealed class UpdateUnitApi(TacticalHeroesApiClient client)
{
    public async Task<Result<UpdateUnitFormModel>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Units[id].GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UpdateUnitMapper.ToForm);
    }

    public async Task<Result<Guid>> UpdateAsync(
        Guid id,
        UpdateUnitFormModel unit,
        CancellationToken cancellationToken)
    {
        var request = UpdateUnitMapper.ToRequest(unit);

        Result result = await client.Api.V1.Units[id].PutAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(() => id);
    }
}
