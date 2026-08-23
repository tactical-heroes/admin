using TacticalHeroes.Admin.Shared.Navigation;

namespace TacticalHeroes.Admin.Modules.Identity;

public static class IdentityRoutes
{
    public const string Login = "/login";

    public const string ConfirmEmail = "/confirm-email";

    public const string ResetPassword = "/reset-password";

    public const string Roles = "/roles";

    public const string CreateRole = "/roles/new";

    public const string RoleTemplate = "/roles/{Id:guid}";

    public const string Users = "/users";

    public const string CreateUser = "/users/new";

    public const string UserTemplate = "/users/{Id:guid}";

    public const string AuthenticationChallenge = "/authentication/challenge";

    public const string AuthenticationSignIn = "/authentication/sign-in";

    public const string AuthenticationLogout = "/authentication/logout";

    public static string Role(Guid id)
    {
        return $"{Roles}/{id:D}";
    }

    public static string User(Guid id)
    {
        return $"{Users}/{id:D}";
    }

    public static string LoginPage(
        string? returnUrl = null,
        LoginMode? mode = null,
        LoginError? error = null)
    {
        return RouteUriBuilder.Build(
            Login,
            new
            {
                mode = mode is null ? null : GetValue(mode.Value),
                returnUrl,
                error = error is null ? null : GetValue(error.Value),
            });
    }

    public static string Challenge(string returnUrl = "/")
    {
        return RouteUriBuilder.Build(AuthenticationChallenge, new { returnUrl });
    }

    public static string ConfirmEmailPage(Guid userId, string emailConfirmationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailConfirmationToken);

        return RouteUriBuilder.Build(
            ConfirmEmail,
            new { userId, emailConfirmationToken });
    }

    public static string ResetPasswordPage(Guid userId, string passwordResetToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordResetToken);

        return RouteUriBuilder.Build(
            ResetPassword,
            new { userId, passwordResetToken });
    }

    private static string GetValue(LoginMode mode)
    {
        return mode switch
        {
            LoginMode.Register => "register",
            LoginMode.Confirmation => "confirmation",
            LoginMode.Recover => "recover",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
        };
    }

    private static string GetValue(LoginError error)
    {
        return error switch
        {
            LoginError.InvalidCredentials => "invalid_credentials",
            LoginError.Forbidden => "forbidden",
            LoginError.InvalidRequest => "invalid_request",
            LoginError.Unavailable => "unavailable",
            LoginError.OAuth => "oauth",
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null),
        };
    }
}

public enum LoginMode
{
    Register,
    Confirmation,
    Recover,
}

public enum LoginError
{
    InvalidCredentials,
    Forbidden,
    InvalidRequest,
    Unavailable,
    OAuth,
}
