using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Errors;

public sealed class ApiErrorMessageTests
{
    [Fact(DisplayName = "Maps only errors for writable form fields")]
    public void FieldErrors_Should_KeepOnlyWritableFormFields()
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

    [Fact(DisplayName = "Treats errors without fields as unhandled")]
    public void UnhandledErrors_Should_IncludeGeneralErrors()
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
