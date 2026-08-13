using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Api;

public sealed class UpdateFactionApi(TacticalHeroesApiClient client)
{
    public async Task<Result<UpdateFactionFormModel>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Factions[id].GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UpdateFactionMapper.ToForm);
    }

    public async Task<Result<Guid>> UpdateAsync(
        Guid id,
        UpdateFactionFormModel faction,
        CancellationToken cancellationToken)
    {
        var request = UpdateFactionMapper.ToRequest(faction);

        Result result = await client.Api.V1.Factions[id].PutAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(() => id);
    }
}
