using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated;
using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Api;

public sealed class AuthApi(TacticalHeroesApiClient client)
{
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
