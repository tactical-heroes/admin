using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;

public sealed class RolesApi(TacticalHeroesApiClient client)
{
    public async Task<Result<Guid>> CreateAsync(
        CreateRoleFormModel role,
        CancellationToken cancellationToken)
    {
        var request = CreateRoleMapper.ToRequest(role);
        var result = await client.Api.V1.Roles.PostAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(CreateRoleMapper.ToId);
    }
}
