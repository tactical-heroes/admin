using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Api;

public sealed class UpdateHeroApi(TacticalHeroesApiClient client)
{
    public async Task<Result<HeroFormModel>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Heroes[id].GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UpdateHeroMapper.ToForm);
    }

    public async Task<Result<Guid>> UpdateAsync(
        Guid id,
        HeroFormModel hero,
        CancellationToken cancellationToken)
    {
        var request = UpdateHeroMapper.ToRequest(hero);

        Result result = await client.Api.V1.Heroes[id].PutAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(() => id);
    }
}
