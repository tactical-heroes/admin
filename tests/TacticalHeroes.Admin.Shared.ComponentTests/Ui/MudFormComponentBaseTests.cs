using FluentValidation;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;
using MudBlazor.Services;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Ui;
using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class MudFormComponentBaseTests : BunitContext
{
    public MudFormComponentBaseTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Saves a valid form once while submission is in progress")]
    public async Task SubmitAsync_Should_SaveOnce_When_FormIsValidAndAlreadySubmitting()
    {
        var component = CreateComponent();
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

    [Fact(DisplayName = "Does not save when the form is unavailable")]
    public async Task SubmitAsync_Should_NotSave_When_FormIsUnavailable()
    {
        var component = CreateComponent(hasForm: false);

        await component.SubmitAsync();

        component.SaveCount.ShouldBe(0);
        component.Saving.ShouldBeFalse();
    }

    [Fact(DisplayName = "Does not save a model rejected by FluentValidation")]
    public async Task SubmitAsync_Should_NotSave_When_ModelValidationFails()
    {
        var component = CreateComponent(isModelValid: false);

        await component.SubmitAsync();

        component.SaveCount.ShouldBe(0);
        component.Saving.ShouldBeFalse();
    }

    private TestComponent CreateComponent(
        bool hasForm = true,
        bool isModelValid = true)
    {
        return new TestComponent(
            hasForm,
            isModelValid,
            Services.GetRequiredService<ISnackbar>(),
            Services.GetRequiredService<NavigationManager>());
    }

    private sealed class TestComponent
        : MudCreateFormComponentBase<TestModel, TestValidator>
    {
        private readonly TestSaveOperation _saveOperation;

        public TestComponent(
            bool hasForm,
            bool isModelValid,
            ISnackbar snackbar,
            NavigationManager navigation)
            : this(
                new TestSaveOperation(),
                hasForm,
                isModelValid,
                snackbar,
                navigation)
        {
        }

        private TestComponent(
            TestSaveOperation saveOperation,
            bool hasForm,
            bool isModelValid,
            ISnackbar snackbar,
            NavigationManager navigation)
            : base(
                saveOperation.SaveAsync,
                "Saved",
                static _ => "/saved",
                snackbar,
                navigation)
        {
            _saveOperation = saveOperation;
            Form = hasForm ? new MudForm() : null;
            Model.Name = isModelValid ? "Valid" : string.Empty;
        }

        public Func<Task> OnSave
        {
            get => _saveOperation.OnSave;
            set => _saveOperation.OnSave = value;
        }

        public TaskCompletionSource SaveStarted => _saveOperation.SaveStarted;

        public int SaveCount => _saveOperation.SaveCount;

        public bool Saving => IsSaving;

        public new Task SubmitAsync()
        {
            return base.SubmitAsync();
        }
    }

    private sealed class TestSaveOperation
    {
        public Func<Task> OnSave { get; set; } = () => Task.CompletedTask;

        public TaskCompletionSource SaveStarted { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public int SaveCount { get; private set; }

        public async Task<Result<Guid>> SaveAsync(
            TestModel _,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SaveCount++;
            SaveStarted.TrySetResult();
            await OnSave();
            return Result.Success(Guid.Empty);
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
