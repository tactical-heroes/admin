using FluentValidation;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Ui;
using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

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
    public async Task SubmitResultAsync_Should_SetError_When_ResultFails()
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

    private sealed class TestComponent
        : MudValidatedFormComponentBase<TestModel, TestValidator>
    {
        public TestComponent()
        {
            Form = new MudForm();
            Model.Name = "Valid";
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
