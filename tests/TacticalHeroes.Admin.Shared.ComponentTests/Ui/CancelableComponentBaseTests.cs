using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class CancelableComponentBaseTests
{
    [Fact(DisplayName = "Cancels the lifetime token when the component is disposed")]
    public void Dispose_Should_CancelLifetimeToken()
    {
        var component = new TestComponent();
        CancellationToken lifetimeToken = component.Token;

        component.Dispose();

        lifetimeToken.IsCancellationRequested.ShouldBeTrue();
    }

    private sealed class TestComponent : CancelableComponentBase
    {
        public CancellationToken Token => LifetimeToken;
    }
}
