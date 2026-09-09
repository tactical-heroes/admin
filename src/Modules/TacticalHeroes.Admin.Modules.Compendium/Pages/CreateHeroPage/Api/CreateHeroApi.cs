using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Api;

public sealed class CreateHeroApi(TacticalHeroesApiClient client)
{
    public async Task<Result<Guid>> CreateAsync(
        HeroFormModel hero,
        CancellationToken cancellationToken)
    {
        var request = CreateHeroMapper.ToRequest(hero);
        var result = await client.Api.V1.Heroes.PostAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(CreateHeroMapper.ToId);
    }
}
