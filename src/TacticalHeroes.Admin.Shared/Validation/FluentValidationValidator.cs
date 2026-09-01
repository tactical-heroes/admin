using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace TacticalHeroes.Admin.Shared.Validation;

public sealed class FluentValidationValidator<TModel> : ComponentBase, IDisposable
    where TModel : class
{
    private EditContext? _editContext;
    private ValidationMessageStore? _messages;

    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    [Parameter, EditorRequired]
    public IValidator<TModel> Validator { get; set; } = null!;

    protected override void OnInitialized()
    {
        _editContext = CurrentEditContext ?? throw new InvalidOperationException(
            $"{nameof(FluentValidationValidator<TModel>)} requires a cascading " +
            $"{nameof(EditContext)} parameter.");

        if (_editContext.Model is not TModel)
        {
            throw new InvalidOperationException(
                $"The edit context model must be {typeof(TModel).FullName}.");
        }

        _messages = new ValidationMessageStore(_editContext);
        _editContext.OnValidationRequested += HandleValidationRequested;
        _editContext.OnFieldChanged += HandleFieldChanged;
    }

    private void HandleValidationRequested(
        object? _,
        ValidationRequestedEventArgs __)
    {
        ValidationResult result = Validator.Validate((TModel)_editContext!.Model);

        _messages!.Clear();
        AddErrors(result);
        _editContext.NotifyValidationStateChanged();
    }

    private void HandleFieldChanged(object? _, FieldChangedEventArgs eventArgs)
    {
        ValidationResult result = Validator.Validate(
            ValidationContext<TModel>.CreateWithOptions(
                (TModel)_editContext!.Model,
                strategy => strategy.IncludeProperties(eventArgs.FieldIdentifier.FieldName)));

        _messages!.Clear(eventArgs.FieldIdentifier);
        AddErrors(result);
        _editContext.NotifyValidationStateChanged();
    }

    private void AddErrors(ValidationResult result)
    {
        foreach (ValidationFailure error in result.Errors)
        {
            _messages!.Add(
                _editContext!.Field(error.PropertyName),
                error.ErrorMessage);
        }
    }

    public void Dispose()
    {
        if (_editContext is not null)
        {
            _editContext.OnValidationRequested -= HandleValidationRequested;
            _editContext.OnFieldChanged -= HandleFieldChanged;
        }
    }
}
