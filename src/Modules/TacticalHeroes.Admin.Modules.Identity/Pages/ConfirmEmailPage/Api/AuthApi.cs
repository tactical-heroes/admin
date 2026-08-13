using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.ConfirmEmailPage.Api;

public sealed class AuthApi(TacticalHeroesApiClient client)
{
    public Task<Result> ConfirmEmailAsync(
        Guid userId,
        string emailConfirmationToken,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Auth.ConfirmEmail.PostAsync(
                new ConfirmEmailRequest
                {
                    UserId = userId,
                    EmailConfirmationToken = emailConfirmationToken,
                },
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }
}
