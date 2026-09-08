namespace TacticalHeroes.Admin.Shared.ComponentTests.Errors;

public sealed class ApiErrorMessageTests
{
    [Fact(DisplayName = "GetFieldErrors should keep only writable form fields when errors include unknown fields")]
    public void GetFieldErrors_Should_KeepOnlyWritableFormFields_When_ErrorsIncludeUnknownFields()
    {
        Error fieldError = Error.Validation("Name is required.").WithField("name");
        Error unknownFieldError = Error.Validation("Identifier is invalid.").WithField("Id");

        IReadOnlyDictionary<string, string[]> errors =
            ApiErrorMessage.GetFieldErrors<TestFormModel>([fieldError, unknownFieldError]);

        errors.Keys.ShouldBe([nameof(TestFormModel.Name)]);
        errors[nameof(TestFormModel.Name)].ShouldBe([fieldError.Message]);
        ApiErrorMessage.GetUnhandledErrors<TestFormModel>([fieldError, unknownFieldError])
            .ShouldBe([unknownFieldError]);
    }

    [Fact(DisplayName = "GetUnhandledErrors should include general errors when errors have no field")]
    public void GetUnhandledErrors_Should_IncludeGeneralErrors_When_ErrorsHaveNoField()
    {
        Error error = Error.Unexpected("API is unavailable.");

        ApiErrorMessage.GetFieldErrors<TestFormModel>([error]).ShouldBeEmpty();
        ApiErrorMessage.GetUnhandledErrors<TestFormModel>([error]).ShouldBe([error]);
    }

    private sealed class TestFormModel
    {
        public string Name { get; set; } = string.Empty;

        public Guid Id { get; }
    }
}
