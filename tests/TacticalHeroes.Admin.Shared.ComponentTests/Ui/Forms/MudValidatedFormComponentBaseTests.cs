using FluentValidation;

using MudBlazor;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Forms;

public sealed class MudValidatedFormComponentBaseTests
{
    [Fact(DisplayName = "Marks a successful result submission as completed")]
    public async Task SubmitResultAsync_Should_Complete_When_ResultSucceeds()
    {
        var component = new TestComponent();

        await component.SubmitResultAsync(
            static _ => Task.FromResult(Result.Success()));

        component.Submitted.ShouldBeTrue();
        component.Error.ShouldBeNull();
        component.Submitting.ShouldBeFalse();
    }

    [Fact(DisplayName = "Exposes an error from a failed result submission")]
    public async Task CompleteResultSubmission_Should_SetError_When_ResultFails()
    {
        var component = new TestComponent();

        await component.SubmitResultAsync(
            static _ => Task.FromResult(
                Result.Failure(Error.Failure("Request failed."))));

        component.Submitted.ShouldBeFalse();
        component.Error.ShouldBe("Request failed.");
        component.Submitting.ShouldBeFalse();
    }

    [Fact(DisplayName = "Supports generic result submissions")]
    public async Task SubmitResultAsync_Should_Complete_When_GenericResultSucceeds()
    {
        var component = new TestComponent();

        await component.SubmitResultAsync(
            static _ => Task.FromResult(Result.Success(Guid.Empty)));

        component.Submitted.ShouldBeTrue();
        component.Error.ShouldBeNull();
    }

    [Fact(DisplayName = "Stops submitting when the operation throws")]
    public async Task SubmitAsync_Should_StopSubmitting_When_OperationThrows()
    {
        var component = new TestComponent();

        await Should.ThrowAsync<InvalidOperationException>(
            () => component.SubmitAsync(
                static _ => throw new InvalidOperationException("Request failed.")));

        component.Submitting.ShouldBeFalse();
    }

    [Theory(DisplayName = "Clears the previous success or error while a new submission is in progress")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PrepareResultSubmission_Should_ClearPreviousResult_When_SubmittingAgain(bool previousSuccess)
    {
        var component = new TestComponent();
        await component.SubmitResultAsync(_ => Task.FromResult(previousSuccess
            ? Result.Success()
            : Result.Failure(Error.Failure("Previous failure."))));
        component.Submitted.ShouldBe(previousSuccess);
        component.Error.ShouldBe(previousSuccess ? null : "Previous failure.");
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var completion = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task submission = component.SubmitResultAsync(_ =>
        {
            started.SetResult();
            return completion.Task;
        });
        await started.Task;

        component.Submitted.ShouldBeFalse();
        component.Error.ShouldBeNull();
        component.Submitting.ShouldBeTrue();

        completion.SetResult(Result.Success());
        await submission;

        component.Submitted.ShouldBeTrue();
        component.Submitting.ShouldBeFalse();
    }

    [Fact(DisplayName = "Invokes a valid submission only once while it is in progress")]
    public async Task SubmitAsync_Should_InvokeOnce_When_AlreadySubmitting()
    {
        var component = new TestComponent();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        Task first = component.SubmitAsync(_ =>
        {
            calls++;
            started.SetResult();
            return completion.Task;
        });
        await started.Task;

        await component.SubmitAsync(_ =>
        {
            calls++;
            return Task.CompletedTask;
        });

        calls.ShouldBe(1);
        component.Submitting.ShouldBeTrue();

        completion.SetResult();
        await first;

        component.Submitting.ShouldBeFalse();
    }

    [Theory(DisplayName = "Does not submit when the form is missing or the model is invalid")]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task SubmitAsync_Should_NotInvoke_When_FormCannotBeValidated(bool hasForm, bool isModelValid)
    {
        var component = new TestComponent(hasForm, isModelValid);
        bool invoked = false;

        await component.SubmitAsync(_ =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        invoked.ShouldBeFalse();
        component.Submitting.ShouldBeFalse();
    }

    private sealed class TestComponent
        : MudValidatedFormComponentBase<TestModel, TestValidator>
    {
        public TestComponent(bool hasForm = true, bool isModelValid = true)
        {
            Form = hasForm ? new MudForm() : null;
            Model.Name = isModelValid ? "Valid" : string.Empty;
        }

        public bool Submitting => IsSubmitting;

        public bool Submitted => IsSubmitted;

        public string? Error => SubmissionError;

        public new Task SubmitAsync(
            Func<CancellationToken, Task> submitAsync)
        {
            return base.SubmitAsync(submitAsync);
        }

        public new Task SubmitResultAsync(
            Func<CancellationToken, Task<Result>> submitAsync)
        {
            return base.SubmitResultAsync(submitAsync);
        }

        public new Task SubmitResultAsync<TValue>(
            Func<CancellationToken, Task<Result<TValue>>> submitAsync)
        {
            return base.SubmitResultAsync(submitAsync);
        }
    }

    private sealed class TestValidator : MudFormValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(model => model.Name)
                .NotEmpty();
        }
    }

    private sealed class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }
}
