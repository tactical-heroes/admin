using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;

public sealed class UsersApi(TacticalHeroesApiClient client)
{
    public async Task<Result<PaginationResult<UserListItem>>> GetPageAsync(
        int pageNumber,
        int pageSize,
        string? email,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Users.GetAsync(
                request =>
                {
                    request.QueryParameters.PageNumber = pageNumber;
                    request.QueryParameters.PageSize = pageSize;
                    request.QueryParameters.Email = string.IsNullOrWhiteSpace(email)
                        ? null
                        : email.Trim();
                },
                cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(response => UsersMapper.ToPage(response, pageNumber, pageSize));
    }

    public async Task<Result<UserDetails>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Users[id].GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UsersMapper.ToDetails);
    }

    public async Task<Result<IReadOnlyList<UserStatus>>> GetStatusesAsync(
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Users.Statuses.GetAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UsersMapper.ToStatuses);
    }

    public async Task<Result<Guid>> CreateAsync(
        UserDetails user,
        CancellationToken cancellationToken)
    {
        var request = UsersMapper.ToCreateRequest(user);
        var result = await client.Api.V1.Users.PostAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(UsersMapper.ToId);
    }

    public async Task<Result> UpdateAsync(
        UserDetails user,
        CancellationToken cancellationToken)
    {
        if (user.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                "A user identifier is required for update.");
        }

        var request = UsersMapper.ToUpdateRequest(user);

        return await client.Api.V1.Users[user.Id].PutAsync(
                request,
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }

    public Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Users[id].DeleteAsync(
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }
}
