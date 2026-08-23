using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;

public sealed class UserStatusApi(TacticalHeroesApiClient client)
{
    public async Task<Result<IReadOnlyList<UserStatus>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Users.Statuses.GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UserStatusMapper.ToValues);
    }
}
