using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Ui;
using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class MudFormComponentBaseTests
{
    [Fact(DisplayName = "Saves a valid form once while submission is in progress")]
    public async Task SubmitAsync_Should_SaveOnce_When_FormIsValidAndAlreadySubmitting()
    {
        var component = new TestComponent(isValid: true);
        var saveCompletion = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        component.OnSave = () => saveCompletion.Task;

        Task firstSubmit = component.SubmitAsync();
        await component.SaveStarted.Task;
        Task secondSubmit = component.SubmitAsync();

        component.SaveCount.ShouldBe(1);
        component.Saving.ShouldBeTrue();
        await secondSubmit;

        saveCompletion.SetResult();
        await firstSubmit;

        component.Saving.ShouldBeFalse();
    }

    [Fact(DisplayName = "Does not save an invalid form")]
    public async Task SubmitAsync_Should_NotSave_When_FormIsInvalid()
    {
        var component = new TestComponent(isValid: false);

        await component.SubmitAsync();

        component.SaveCount.ShouldBe(0);
        component.Saving.ShouldBeFalse();
    }

    private sealed class TestComponent
        : MudFormComponentBase<TestModel, TestValidator, Result>
    {
        public TestComponent(bool isValid)
        {
            Form = new MudForm();
            IsValid = isValid;
        }

        public Func<Task> OnSave { get; set; } = () => Task.CompletedTask;

        public TaskCompletionSource SaveStarted { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public int SaveCount { get; private set; }

        public bool Saving => IsSaving;

        public new Task SubmitAsync()
        {
            return base.SubmitAsync();
        }

        protected override async Task<Result> SaveCoreAsync()
        {
            SaveCount++;
            SaveStarted.TrySetResult();
            await OnSave();
            return Result.Success();
        }
    }

    private sealed class TestModel
    {
    }

    private sealed class TestValidator : MudFormValidator<TestModel>
    {
    }
}
