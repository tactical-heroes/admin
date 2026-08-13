using FluentValidation;
using FluentValidation.Results;

namespace TacticalHeroes.Admin.Shared.Validation;

public abstract class MudFormValidator<TModel> : AbstractValidator<TModel>
    where TModel : class
{
    protected MudFormValidator()
    {
        ValidateValue = ValidateProperty;
    }

    public Func<object, string, IEnumerable<string>> ValidateValue { get; }

    private IEnumerable<string> ValidateProperty(object model, string propertyName)
    {
        ValidationResult result = Validate(
            ValidationContext<TModel>.CreateWithOptions(
                (TModel)model,
                strategy => strategy.IncludeProperties(propertyName)));

        return result.Errors.Select(error => error.ErrorMessage);
    }
}
