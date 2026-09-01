using FluentValidation;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Validation;

public sealed class FluentValidationValidatorTests : BunitContext
{
    [Fact(DisplayName = "Adds errors when the edit context requests validation")]
    public void ValidationRequested_Should_AddErrors_When_ModelIsInvalid()
    {
        var model = new TestModel();
        var editContext = new EditContext(model);
        RenderValidator(editContext);

        bool isValid = editContext.Validate();

        isValid.ShouldBeFalse();
        editContext
            .GetValidationMessages(editContext.Field(nameof(TestModel.Name)))
            .ShouldBe(["Name is required."]);
    }

    [Fact(DisplayName = "Updates errors when a field changes")]
    public void FieldChanged_Should_UpdateErrors_When_FieldValueChanges()
    {
        var model = new TestModel();
        var editContext = new EditContext(model);
        RenderValidator(editContext);
        FieldIdentifier field = editContext.Field(nameof(TestModel.Name));

        editContext.NotifyFieldChanged(field);
        editContext.GetValidationMessages(field).ShouldBe(["Name is required."]);

        model.Name = "Hero";
        editContext.NotifyFieldChanged(field);
        editContext.GetValidationMessages(field).ShouldBeEmpty();
    }

    private void RenderValidator(EditContext editContext)
    {
        Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(parameter => parameter.Value, editContext)
            .AddChildContent<FluentValidationValidator<TestModel>>(child => child
                .Add(parameter => parameter.Validator, new TestModelValidator())));
    }

    private sealed class TestModelValidator : AbstractValidator<TestModel>
    {
        public TestModelValidator()
        {
            RuleFor(model => model.Name)
                .NotEmpty()
                .WithMessage("Name is required.");
        }
    }

    private sealed class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }
}
