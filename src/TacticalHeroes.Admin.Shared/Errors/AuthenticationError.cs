using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TacticalHeroes.Admin.Shared.Errors;

public enum AuthenticationError
{
    [Display(Name = "Invalid email or password.")]
    InvalidCredentials = 0,

    [Display(Name = "The account is not confirmed, blocked, or temporarily locked after failed sign-in attempts.")]
    Forbidden = 1,

    [Display(Name = "The sign-in link is invalid or has expired. Start the sign-in process again.")]
    InvalidRequest = 2,

    [Display(Name = "The authentication service is temporarily unavailable. Please try again later.")]
    Unavailable = 3,

    [JsonStringEnumMemberName("oauth")]
    [Display(Name = "OAuth sign-in could not be completed. Start the sign-in process again.")]
    OAuth = 4,
}
