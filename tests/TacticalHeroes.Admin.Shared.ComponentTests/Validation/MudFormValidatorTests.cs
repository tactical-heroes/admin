using FluentValidation;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Validation;

public sealed class MudFormValidatorTests
{
    [Fact(DisplayName = "ValidateValue should return errors only for requested property when model has invalid fields")]
    public void ValidateValue_Should_ReturnErrorsOnlyForRequestedProperty_When_ModelHasInvalidFields()
    {
        var validator = new TestModelValidator();
        var model = new TestModel();

        string[] errors = [.. validator.ValidateValue(model, nameof(TestModel.Name))];

        errors.ShouldBe(["Name is required."]);
    }

    [Fact(DisplayName = "For should return errors only for selected property when property is selected")]
    public void For_Should_ReturnErrorsOnlyForSelectedProperty_When_PropertyIsSelected()
    {
        var validator = new TestModelValidator();
        var model = new TestModel();

        string[] errors = [.. validator
            .For(model, static model => model.Name)(model.Name)];

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
