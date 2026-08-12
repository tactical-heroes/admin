using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Auth.Api;

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
                    Email = email.Trim(),
                    UserName = userName.Trim(),
                    Password = password,
                },
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);

        return result.Map(response => response!.Id!.Value);
    }

    public Task<Result> ResendConfirmationEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Auth.ResendConfirmationEmail.PostAsync(
                new ResendConfirmationEmailRequest { Email = email.Trim() },
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }

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

    public Task<Result> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Auth.ForgotPassword.PostAsync(
                new ForgotPasswordRequest { Email = email.Trim() },
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }

    public Task<Result> ResetPasswordAsync(
        Guid userId,
        string passwordResetToken,
        string newPassword,
        CancellationToken cancellationToken)
    {
        return client.Api.V1.Auth.ResetPassword.PostAsync(
                new ResetPasswordRequest
                {
                    UserId = userId,
                    PasswordResetToken = passwordResetToken,
                    NewPassword = newPassword,
                },
                cancellationToken: cancellationToken)
            .ToApiResultAsync(cancellationToken);
    }
}
