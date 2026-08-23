using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api;

public sealed class UpdateUserApi(TacticalHeroesApiClient client)
{
    public async Task<Result<UpdateUserLoadState>> GetStateAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Task<Result<UpdateUserFormModel>> userTask = GetUserAsync(
            id,
            cancellationToken);
        Task<Result<IReadOnlyList<UserStatus>>> statusesTask =
            GetStatusesAsync(cancellationToken);

        await Task.WhenAll(userTask, statusesTask);

        Result<UpdateUserFormModel> userResult = await userTask;
        Result<IReadOnlyList<UserStatus>> statusesResult = await statusesTask;

        return ResultCombiner.Combine(userResult, statusesResult)
            .Map(static state => new UpdateUserLoadState(state.Item1, state.Item2));
    }

    private async Task<Result<UpdateUserFormModel>> GetUserAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Users[id].GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UpdateUserMapper.ToForm);
    }

    private async Task<Result<IReadOnlyList<UserStatus>>> GetStatusesAsync(
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Users.Statuses.GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UpdateUserMapper.ToStatuses);
    }

    public async Task<Result<Guid>> UpdateAsync(
        Guid id,
        UpdateUserFormModel user,
        CancellationToken cancellationToken)
    {
        var request = UpdateUserMapper.ToRequest(user);

        Result result = await client.Api.V1.Users[id].PutAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(() => id);
    }
}
