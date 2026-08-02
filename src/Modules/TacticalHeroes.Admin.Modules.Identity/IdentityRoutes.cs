using System.Globalization;

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

    public static string RolesPage(int pageNumber = 1, int pageSize = 10)
    {
        ValidatePageNumber(pageNumber);
        ValidatePageSize(pageSize);

        return BuildUri(
            Roles,
            ("page", pageNumber == 1
                ? null
                : pageNumber.ToString(CultureInfo.InvariantCulture)),
            ("pageSize", pageSize == 10
                ? null
                : pageSize.ToString(CultureInfo.InvariantCulture)));
    }

    public static string UsersPage(
        string? email = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        ValidatePageNumber(pageNumber);
        ValidatePageSize(pageSize);

        return BuildUri(
            Users,
            ("email", string.IsNullOrWhiteSpace(email) ? null : email.Trim()),
            ("page", pageNumber == 1
                ? null
                : pageNumber.ToString(CultureInfo.InvariantCulture)),
            ("pageSize", pageSize == 10
                ? null
                : pageSize.ToString(CultureInfo.InvariantCulture)));
    }

    public static string LoginPage(
        string? returnUrl = null,
        LoginMode? mode = null,
        LoginError? error = null)
    {
        return BuildUri(
            Login,
            ("mode", mode is null ? null : GetValue(mode.Value)),
            ("returnUrl", returnUrl),
            ("error", error is null ? null : GetValue(error.Value)));
    }

    public static string Challenge(string returnUrl = "/")
    {
        return BuildUri(AuthenticationChallenge, ("returnUrl", returnUrl));
    }

    public static string ConfirmEmailPage(Guid userId, string emailConfirmationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailConfirmationToken);

        return BuildUri(
            ConfirmEmail,
            ("userId", userId.ToString("D")),
            ("emailConfirmationToken", emailConfirmationToken));
    }

    public static string ResetPasswordPage(Guid userId, string passwordResetToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordResetToken);

        return BuildUri(
            ResetPassword,
            ("userId", userId.ToString("D")),
            ("passwordResetToken", passwordResetToken));
    }

    private static string BuildUri(
        string path,
        params (string Name, string? Value)[] parameters)
    {
        string[] query = parameters
            .Where(static parameter => !string.IsNullOrWhiteSpace(parameter.Value))
            .Select(static parameter =>
                $"{Uri.EscapeDataString(parameter.Name)}=" +
                Uri.EscapeDataString(parameter.Value!))
            .ToArray();

        return query.Length == 0
            ? path
            : $"{path}?{string.Join('&', query)}";
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

    private static void ValidatePageNumber(int pageNumber)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                pageNumber,
                "Page number must be greater than zero.");
        }
    }

    private static void ValidatePageSize(int pageSize)
    {
        if (pageSize is not (10 or 25 or 50 or 100))
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                "Page size must be 10, 25, 50, or 100.");
        }
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
