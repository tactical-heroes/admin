using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;
using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Forms;

public sealed class MudCreateFormComponentBaseTests : BunitContext
{
    [Fact(DisplayName = "A create form passes its model and lifetime token to the create operation")]
    public async Task SubmitAsync_Should_CreateAndNavigate_When_FormIsValid()
    {
        Services.AddMudServices();
        Guid id = Guid.NewGuid();
        TestModel? saved = null;
        using var component = new TestComponent((model, token) =>
        {
            token.CanBeCanceled.ShouldBeTrue();
            saved = model;
            return Task.FromResult(Result.Success(id));
        }, Services.GetRequiredService<ISnackbar>(), Services.GetRequiredService<NavigationManager>());

        await component.SubmitFormAsync();

        saved.ShouldBeSameAs(component.Model);
        Services.GetRequiredService<NavigationManager>().Uri.ShouldEndWith("/items/" + id);
    }

    private sealed class TestComponent(
        Func<TestModel, CancellationToken, Task<Result<Guid>>> create,
        ISnackbar snackbar,
        NavigationManager navigation)
        : MudCreateFormComponentBase<TestModel, TestValidator>(create, "Created", id => "/items/" + id, snackbar, navigation)
    {
        public Task SubmitFormAsync()
        {
            Form = new MudForm();
            return SubmitAsync();
        }
    }

    private sealed class TestModel { }

    private sealed class TestValidator : MudFormValidator<TestModel> { }
}
