using System.Linq.Expressions;

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

    public Func<TProperty, IEnumerable<string>> For<TProperty>(
        TModel model,
        Expression<Func<TModel, TProperty>> property)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(property);

        string propertyPath = GetPropertyPath(property);

        return _ => ValidateProperty(model, propertyPath);
    }

    private IEnumerable<string> ValidateProperty(object model, string propertyName)
    {
        ValidationResult result = Validate(
            ValidationContext<TModel>.CreateWithOptions(
                (TModel)model,
                strategy => strategy.IncludeProperties(propertyName)));

        return result.Errors.Select(error => error.ErrorMessage);
    }

    private static string GetPropertyPath<TProperty>(
        Expression<Func<TModel, TProperty>> property)
    {
        var members = new Stack<string>();
        Expression? expression = property.Body;

        while (expression is MemberExpression member)
        {
            members.Push(member.Member.Name);
            expression = member.Expression;
        }

        if (members.Count == 0 || expression != property.Parameters[0])
        {
            throw new ArgumentException(
                "The expression must select a model property.",
                nameof(property));
        }

        return string.Join('.', members);
    }
}
