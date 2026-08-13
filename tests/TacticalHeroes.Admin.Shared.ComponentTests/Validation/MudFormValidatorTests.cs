using FluentValidation;

using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Validation;

public sealed class MudFormValidatorTests
{
    [Fact(DisplayName = "Validates only the field requested by MudForm")]
    public void ValidateValue_Should_ReturnErrorsOnlyForRequestedProperty()
    {
        var validator = new TestModelValidator();
        var model = new TestModel();

        string[] errors = validator
            .ValidateValue(model, nameof(TestModel.Name))
            .ToArray();

        errors.ShouldBe(["Name is required."]);
    }

    private sealed class TestModelValidator : MudFormValidator<TestModel>
    {
        public TestModelValidator()
        {
            RuleFor(model => model.Name)
                .NotEmpty()
                .WithMessage("Name is required.");

            RuleFor(model => model.Description)
                .NotEmpty()
                .WithMessage("Description is required.");
        }
    }

    private sealed class TestModel
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
