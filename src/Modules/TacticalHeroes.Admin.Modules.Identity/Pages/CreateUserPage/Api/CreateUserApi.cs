using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Api;

public sealed class CreateUserApi(TacticalHeroesApiClient client)
{
    public async Task<Result<IReadOnlyList<UserStatus>>> GetStatusesAsync(
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Users.Statuses.GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(CreateUserMapper.ToStatuses);
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateUserFormModel user,
        CancellationToken cancellationToken)
    {
        var request = CreateUserMapper.ToRequest(user);
        var result = await client.Api.V1.Users.PostAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(CreateUserMapper.ToId);
    }
}
