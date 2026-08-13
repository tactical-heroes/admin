using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Api;

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
                    request.QueryParameters.Email = email;
                },
                cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(response => UserListMapper.ToPage(response, pageNumber, pageSize));
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
