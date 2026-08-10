using Microsoft.Kiota.Abstractions.Serialization;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Api.UnitTests.Errors;

public sealed class ApiResultTests
{
    [Fact(DisplayName = "Returns a successful result with a value")]
    public async Task ExecuteAsync_Should_ReturnSuccess_When_OperationSucceeds()
    {
        Result<int> result = await ApiResult.ExecuteAsync(
            () => Task.FromResult(42),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact(DisplayName = "Maps validation messages and their fields")]
    public async Task ExecuteAsync_Should_MapFieldErrors_When_ServerReturnsValidationProblem()
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

        Result result = await ApiResult.ExecuteAsync(
            () => Task.FromException(exception),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        ApiErrorMessage.GetFieldErrors(result.Errors)["Name"].ShouldBe(
        [
            "Role name is required.",
            "Role name must be unique.",
        ]);
        ApiErrorMessage.GetFieldErrors(result.Errors, static _ => null).ShouldBeEmpty();
        ApiErrorMessage.GetUnhandledErrors(result.Errors, static _ => null)
            .ShouldBe(result.Errors);
    }

    [Fact(DisplayName = "Maps problem detail and status to a typed error")]
    public async Task ExecuteAsync_Should_MapTypedError_When_ServerReturnsProblem()
    {
        var exception = new ProblemDetails
        {
            ResponseStatusCode = 409,
            Detail = "A role with this name already exists.",
        };

        Result result = await ApiResult.ExecuteAsync(
            () => Task.FromException(exception),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Conflict);
        result.FirstError.Message.ShouldBe("A role with this name already exists.");
    }

    [Fact(DisplayName = "Hides server details for server errors")]
    public async Task ExecuteAsync_Should_HideDetail_When_ServerErrorOccurs()
    {
        var exception = new ProblemDetails
        {
            ResponseStatusCode = 500,
            Detail = "Database connection failed.",
        };

        Result result = await ApiResult.ExecuteAsync(
            () => Task.FromException(exception),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Unexpected);
        result.FirstError.Message.ShouldBe(
            "API временно недоступен. Попробуйте повторить запрос позже.");
    }

    [Fact(DisplayName = "Preserves caller cancellation")]
    public async Task ExecuteAsync_Should_Throw_When_CallerCancelsOperation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            ApiResult.ExecuteAsync(
                () => Task.FromCanceled(cancellationTokenSource.Token),
                cancellationTokenSource.Token));
    }
}
