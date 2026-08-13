using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Api;

public sealed class RolesApi(TacticalHeroesApiClient client)
{
    public async Task<Result<UpdateRoleFormModel>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Roles[id].GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UpdateRoleMapper.ToForm);
    }

    public Task<Result> UpdateAsync(
        Guid id,
        UpdateRoleFormModel role,
        CancellationToken cancellationToken)
    {
        var request = UpdateRoleMapper.ToRequest(role);

        return client.Api.V1.Roles[id].PutAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }
}
