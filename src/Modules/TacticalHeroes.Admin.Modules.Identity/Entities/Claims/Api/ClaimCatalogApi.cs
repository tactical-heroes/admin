using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;

public sealed class ClaimCatalogApi(TacticalHeroesApiClient client)
{
    public async Task<Result<ClaimCatalog>> GetPermissionsAsync(CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Claims.Permissions.GetAsync(cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(ClaimCatalogMapper.ToPermissions);
    }
}
