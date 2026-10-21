using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;

public sealed class RoleOptionsApi(TacticalHeroesApiClient client)
{
    public async Task<Result<IReadOnlyList<RoleOption>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Roles.SelectOptions.GetAsync(cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(RoleOptionsMapper.ToOptions);
    }
}
