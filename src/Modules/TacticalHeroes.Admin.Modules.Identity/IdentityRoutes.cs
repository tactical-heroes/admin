using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;
using TacticalHeroes.Admin.Shared.Navigation;

namespace TacticalHeroes.Admin.Modules.Identity;

public static class IdentityRoutes
{
    public const string Authentication = "/authentication";

    public const string Login = "/login";

    public const string ConfirmEmail = "/confirm-email";

    public const string ResetPassword = "/reset-password";

    public const string Roles = "/roles";

    public const string CreateRole = $"{Roles}/new";

    public const string RoleTemplate = $"{Roles}/{{Id:guid}}";

    public const string Users = "/users";

    public const string CreateUser = $"{Users}/new";

    public const string UserTemplate = $"{Users}/{{Id:guid}}";

    public const string AuthenticationChallenge = $"{Authentication}/challenge";

    public const string AuthenticationSignIn = $"{Authentication}/sign-in";

    public const string AuthenticationLogout = $"{Authentication}/logout";

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
        AuthenticationError? error = null)
    {
        return RouteUriBuilder.Build(
            Login,
            new
            {
                mode,
                returnUrl,
                error,
            });
    }

    public static string Challenge(string returnUrl = "/")
    {
        return RouteUriBuilder.Build(AuthenticationChallenge, new { returnUrl });
    }

    public static string ConfirmEmailPage(Guid userId, string emailConfirmationToken)
    {
        return RouteUriBuilder.Build(
            ConfirmEmail,
            new { userId, emailConfirmationToken });
    }

    public static string ResetPasswordPage(Guid userId, string passwordResetToken)
    {
        return RouteUriBuilder.Build(
            ResetPassword,
            new { userId, passwordResetToken });
    }
}
