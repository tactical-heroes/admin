using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;

public sealed class RoleOptionsApi(TacticalHeroesApiClient client)
{
    public async Task<Result<IReadOnlyList<RoleOption>>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<RoleOption> roles = [];
        int pageNumber = 1;
        int totalPages;

        do
        {
            var result = await client.Api.V1.Roles.GetAsync(
                    request =>
                    {
                        request.QueryParameters.PageNumber = pageNumber;
                        request.QueryParameters.PageSize = 100;
                        request.QueryParameters.Fields = ["name:asc", "id:asc"];
                    },
                    cancellationToken)
                .ToApiResultAsync(cancellationToken);

            if (result.IsFailure)
            {
                return Result.Failure<IReadOnlyList<RoleOption>>(result.Errors);
            }

            PaginationResult<RoleOption> page = RoleOptionsMapper.ToPage(result.Value);
            roles.AddRange(page.Items);
            totalPages = page.TotalPages;
            pageNumber++;
        }
        while (pageNumber <= totalPages);

        return Result.Success<IReadOnlyList<RoleOption>>(roles);
    }
}
