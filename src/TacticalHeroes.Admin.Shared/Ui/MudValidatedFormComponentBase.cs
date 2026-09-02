using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.Ui;

public abstract class MudValidatedFormComponentBase<TModel, TValidator>
    : CancelableComponentBase
    where TModel : class, new()
    where TValidator : MudFormValidator<TModel>, new()
{
    private TModel? _model;

    public virtual TModel Model
    {
        get => _model ??= new();
        set => _model = value;
    }

    protected TValidator Validator { get; } = new();

    protected MudForm? Form { get; set; }

    protected bool IsValid { get; set; }

    protected bool IsSubmitting { get; private set; }

    protected bool IsSubmitted { get; private set; }

    protected string? SubmissionError { get; private set; }

    protected async Task SubmitAsync(
        Func<CancellationToken, Task> submitAsync)
    {
        ArgumentNullException.ThrowIfNull(submitAsync);

        if (Form is null || IsSubmitting)
        {
            return;
        }

        IsSubmitting = true;

        try
        {
            if (await Validator.ValidateFormAsync(Form, Model, LifetimeToken))
            {
                await submitAsync(LifetimeToken);
            }
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    protected Task SubmitResultAsync(
        Func<CancellationToken, Task<Result>> submitAsync)
    {
        ArgumentNullException.ThrowIfNull(submitAsync);

        return SubmitAsync(async cancellationToken =>
        {
            PrepareResultSubmission();
            CompleteResultSubmission(await submitAsync(cancellationToken));
        });
    }

    protected Task SubmitResultAsync<TValue>(
        Func<CancellationToken, Task<Result<TValue>>> submitAsync)
    {
        ArgumentNullException.ThrowIfNull(submitAsync);

        return SubmitAsync(async cancellationToken =>
        {
            PrepareResultSubmission();
            CompleteResultSubmission(await submitAsync(cancellationToken));
        });
    }

    private void PrepareResultSubmission()
    {
        IsSubmitted = false;
        SubmissionError = null;
    }

    private void CompleteResultSubmission(Result result)
    {
        if (result.IsFailure)
        {
            SubmissionError = ApiErrorMessage.FromErrors(result.Errors);
            return;
        }

        IsSubmitted = true;
    }
}
