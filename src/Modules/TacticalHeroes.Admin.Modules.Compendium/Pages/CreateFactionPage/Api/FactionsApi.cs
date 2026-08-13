using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;

public sealed class FactionsApi(TacticalHeroesApiClient client)
{
    public async Task<Result<Guid>> CreateAsync(
        CreateFactionFormModel faction,
        CancellationToken cancellationToken)
    {
        var request = CreateFactionMapper.ToRequest(faction);
        var result = await client.Api.V1.Factions.PostAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(CreateFactionMapper.ToId);
    }
}
