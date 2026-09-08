using Microsoft.Kiota.Abstractions.Serialization;

using Polly.Timeout;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Api.UnitTests.Errors;

public sealed class ApiResultExtensionsTests
{
    [Fact(DisplayName = "ToApiResultAsync should return success when operation succeeds")]
    public async Task ToApiResultAsync_Should_ReturnSuccess_When_OperationSucceeds()
    {
        Result<string> result = await Task.FromResult<string?>("response").ToApiResultAsync(
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("response");
    }

    [Fact(DisplayName = "ToApiResultAsync should return failure when response is null")]
    public async Task ToApiResultAsync_Should_ReturnFailure_When_ResponseIsNull()
    {
        Result<string> result = await Task.FromResult<string?>(null).ToApiResultAsync(
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Unexpected);
        result.FirstError.Message.ShouldBe("API вернул пустой ответ.");
    }

    [Fact(DisplayName = "ToApiResultAsync should map field errors when server returns validation problem")]
    public async Task ToApiResultAsync_Should_MapFieldErrors_When_ServerReturnsValidationProblem()
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

        Result result = await Task.FromException(exception).ToApiResultAsync(
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        result.Errors
            .Select(error => error.Metadata[Error.FieldMetadataKey])
            .ShouldAllBe(field => string.Equals(field as string, "Name", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "ToApiResultAsync should map typed error when server returns problem")]
    public async Task ToApiResultAsync_Should_MapTypedError_When_ServerReturnsProblem()
    {
        var exception = new ProblemDetails
        {
            ResponseStatusCode = 409,
            Detail = "A role with this name already exists.",
        };

        Result result = await Task.FromException(exception).ToApiResultAsync(
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Conflict);
        result.FirstError.Message.ShouldBe("A role with this name already exists.");
    }

    [Fact(DisplayName = "ToApiResultAsync should hide detail when server error occurs")]
    public async Task ToApiResultAsync_Should_HideDetail_When_ServerErrorOccurs()
    {
        var exception = new ProblemDetails
        {
            ResponseStatusCode = 500,
            Detail = "Database connection failed.",
        };

        Result result = await Task.FromException(exception).ToApiResultAsync(
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Unexpected);
        result.FirstError.Message.ShouldBe(
            "API временно недоступен. Попробуйте повторить запрос позже.");
    }

    [Fact(DisplayName = "ToApiResultAsync should throw when caller cancels operation")]
    public async Task ToApiResultAsync_Should_Throw_When_CallerCancelsOperation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            Task.FromCanceled(cancellationTokenSource.Token).ToApiResultAsync(
                cancellationTokenSource.Token));
    }

    [Fact(DisplayName = "ToApiResultAsync should throw when operation has programming error")]
    public async Task ToApiResultAsync_Should_Throw_When_OperationHasProgrammingError()
    {
        var exception = new InvalidOperationException("Mapping failed.");

        InvalidOperationException thrown = await Should.ThrowAsync<InvalidOperationException>(
            () => Task.FromException(exception).ToApiResultAsync(
                TestContext.Current.CancellationToken));

        thrown.ShouldBeSameAs(exception);
    }

    [Fact(DisplayName = "ToApiResultAsync should map unexpected error when resilience timeout occurs")]
    public async Task ToApiResultAsync_Should_MapUnexpectedError_When_ResilienceTimeoutOccurs()
    {
        Result result = await Task.FromException(new TimeoutRejectedException())
            .ToApiResultAsync(TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Unexpected);
    }
}
