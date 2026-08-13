using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;

public sealed class AuthApi(TacticalHeroesApiClient client)
{
    public async Task<Result<Guid>> RegisterAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken)
    {
        var result = await client.Api.V1.Auth.Register.PostAsync(
                new RegisterUserRequest
                {
                    Email = email,
                    UserName = userName,
                    Password = password,
                },
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(AuthMapper.ToId);
    }

    public Task<Result> ResendConfirmationEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Auth.ResendConfirmationEmail.PostAsync(
                new ResendConfirmationEmailRequest { Email = email },
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }

    public Task<Result> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Auth.ForgotPassword.PostAsync(
                new ForgotPasswordRequest { Email = email },
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }
}
