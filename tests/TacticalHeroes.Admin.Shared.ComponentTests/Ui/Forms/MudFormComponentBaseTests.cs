using FluentValidation;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;
using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Forms;

public sealed class MudFormComponentBaseTests : BunitContext
{
    public MudFormComponentBaseTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Forwards the component lifetime token to the save operation")]
    public async Task SubmitAsync_Should_ForwardLifetimeToken_When_FormIsValid()
    {
        var component = CreateComponent();
        CancellationToken? receivedToken = null;

        await component.SubmitAsync(token =>
        {
            receivedToken = token;
            return Task.FromResult(Result.Success(Guid.Empty));
        });

        receivedToken.ShouldBe(component.Token);
    }

    [Theory(DisplayName = "Shows the save result and navigates only after a successful save")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveAsync_Should_ShowResultAndNavigateOnSuccess_When_SaveCompletes(bool fails)
    {
        var component = CreateComponent();
        NavigationManager navigation = Services.GetRequiredService<NavigationManager>();
        string originalUri = navigation.Uri;
        Guid savedId = Guid.NewGuid();
        Result<Guid> result = fails
            ? Result.Failure<Guid>(Error.Failure("Save failed."))
            : Result.Success(savedId);

        await component.SubmitAsync(_ => Task.FromResult(result));

        Snackbar notification = Services.GetRequiredService<ISnackbar>().ShownSnackbars.Single();
        notification.Message.ShouldBe(fails ? "Save failed." : "Saved");
        notification.Severity.ShouldBe(fails ? MudBlazor.Severity.Error : MudBlazor.Severity.Success);
        navigation.Uri.ShouldBe(fails ? originalUri : $"http://localhost/saved/{savedId}");
    }

    private TestComponent CreateComponent()
    {
        return new TestComponent(
            Services.GetRequiredService<ISnackbar>(),
            Services.GetRequiredService<NavigationManager>());
    }

    private sealed class TestComponent : MudFormComponentBase<TestModel, TestValidator>
    {
        public TestComponent(ISnackbar snackbar, NavigationManager navigation)
            : base(snackbar, navigation, "Saved", static id => $"/saved/{id}")
        {
            Form = new MudForm();
            Model.Name = "Valid";
        }

        public CancellationToken Token => LifetimeToken;

        public new Task SubmitAsync(Func<CancellationToken, Task<Result<Guid>>> saveAsync)
        {
            return base.SubmitAsync(saveAsync);
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
