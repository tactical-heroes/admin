using Microsoft.Extensions.DependencyInjection;

using MudBlazor;
using MudBlazor.Services;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class AsyncEnumerationSelectTests : BunitContext
{
    private readonly TestEnumerationProvider _provider = new();

    public AsyncEnumerationSelectTests()
    {
        Services.AddMudServices();
        Services.AddScoped<IEnumerationProvider<TestEnumeration>>(_ => _provider);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Loads enumeration items and selects the first one by default")]
    public void Load_Should_SelectFirstItem_When_DefaultIsEnabled()
    {
        string? selectedItem = null;

        var component = Render<AsyncEnumerationSelect<TestEnumeration>>(
            parameters => parameters
                .Add(select => select.Label, "Статус")
                .Add(select => select.UseFirstAsDefault, true)
                .Add(select => select.ValueChanged, value => selectedItem = value));

        component.WaitForAssertion(() =>
        {
            selectedItem.ShouldBe("active");
            component.FindComponents<MudSelectItem<string>>()
                .Select(item => item.Instance.Value)
                .ShouldBe(["active", "blocked"]);
        });
    }

    [Fact(DisplayName = "Shows a load error and retries loading enumeration items")]
    public void Load_Should_Retry_When_FirstRequestFails()
    {
        _provider.FailNextRequest = true;
        var component = Render<AsyncEnumerationSelect<TestEnumeration>>(
            parameters => parameters
                .Add(select => select.Label, "Статус"));

        component.WaitForAssertion(() =>
            component.Markup.ShouldContain("Повторить"));

        component.FindAll("button")
            .Single(button => button.TextContent.Contains(
                "Повторить",
                StringComparison.Ordinal))
            .Click();

        component.WaitForAssertion(() =>
        {
            _provider.RequestCount.ShouldBe(2);
            component.FindComponents<MudSelectItem<string>>().Count.ShouldBe(2);
        });
    }

    private sealed class TestEnumerationProvider
        : IEnumerationProvider<TestEnumeration>
    {
        public bool FailNextRequest { get; set; }

        public int RequestCount { get; private set; }

        public Task<Result<IReadOnlyList<TestEnumeration>>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            RequestCount++;

            if (FailNextRequest)
            {
                FailNextRequest = false;
                return Task.FromResult(
                    Result.Failure<IReadOnlyList<TestEnumeration>>(
                        Error.Failure("Items cannot be loaded.")));
            }

            IReadOnlyList<TestEnumeration> items =
            [
                new("active", "Active"),
                new("blocked", "Blocked"),
            ];

            return Task.FromResult(Result.Success(items));
        }
    }

    private sealed record TestEnumeration(
        string Name,
        string DisplayName) : IEnumeration;
}
