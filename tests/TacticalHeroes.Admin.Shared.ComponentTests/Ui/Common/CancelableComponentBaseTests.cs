namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Common;

public sealed class CancelableComponentBaseTests
{
    [Fact(DisplayName = "Dispose should cancel only once when disposal is repeated")]
    public void Dispose_Should_CancelOnlyOnce_When_DisposalIsRepeated()
    {
        var component = new TestComponent();
        CancellationToken token = component.Token;

        component.Release(disposing: false);
        token.IsCancellationRequested.ShouldBeFalse();
        component.Release(disposing: true);
        component.Dispose();

        token.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact(DisplayName = "Dispose should cancel lifetime token when component is disposed")]
    public void Dispose_Should_CancelLifetimeToken_When_ComponentIsDisposed()
    {
        var component = new TestComponent();
        CancellationToken lifetimeToken = component.Token;

        component.Dispose();

        lifetimeToken.IsCancellationRequested.ShouldBeTrue();
    }

    private sealed class TestComponent : CancelableComponentBase
    {
        public CancellationToken Token => LifetimeToken;

        public void Release(bool disposing) => Dispose(disposing);
    }
}
