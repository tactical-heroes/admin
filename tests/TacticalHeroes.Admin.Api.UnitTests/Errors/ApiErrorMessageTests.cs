using Microsoft.Kiota.Abstractions.Serialization;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Api.UnitTests.Errors;

public sealed class ApiErrorMessageTests
{
    [Fact(DisplayName = "Returns server detail for client errors")]
    public void FromException_Should_ReturnServerDetail_When_ClientErrorContainsDetail()
    {
        var exception = new ProblemDetails
        {
            ResponseStatusCode = 409,
            Detail = "A role with this name already exists.",
        };

        var message = ApiErrorMessage.FromException(exception);

        message.ShouldBe("A role with this name already exists.");
    }

    [Fact(DisplayName = "Returns validation messages for field errors")]
    public void FromException_Should_ReturnValidationMessages_When_ServerReturnsFieldErrors()
    {
        var exception = new HttpValidationProblemDetails
        {
            ResponseStatusCode = 400,
            Errors = new HttpValidationProblemDetails_errors
            {
                AdditionalData =
                {
                    ["Name"] = new UntypedArray(
                    [
                        new UntypedString("Role name is required."),
                        new UntypedString("Role name must be unique."),
                    ]),
                },
            },
        };

        var message = ApiErrorMessage.FromException(exception);

        message.ShouldBe("Role name is required. Role name must be unique.");
    }

    [Fact(DisplayName = "Returns fallback when client error has no detail")]
    public void FromException_Should_ReturnFallback_When_ClientErrorHasNoDetail()
    {
        var exception = new ProblemDetails
        {
            ResponseStatusCode = 409,
        };

        var message = ApiErrorMessage.FromException(exception);

        message.ShouldBe("Изменения конфликтуют с текущим состоянием данных.");
    }

    [Fact(DisplayName = "Hides server detail for server errors")]
    public void FromException_Should_HideServerDetail_When_ServerErrorOccurs()
    {
        var exception = new ProblemDetails
        {
            ResponseStatusCode = 500,
            Detail = "Database connection failed.",
        };

        var message = ApiErrorMessage.FromException(exception);

        message.ShouldBe("API временно недоступен. Попробуйте повторить запрос позже.");
    }
}
