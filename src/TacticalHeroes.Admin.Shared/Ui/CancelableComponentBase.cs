using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public abstract class CancelableComponentBase : ComponentBase, IDisposable
{
    private readonly CancellationTokenSource _lifetimeCancellationTokenSource = new();

    protected CancellationToken LifetimeToken => _lifetimeCancellationTokenSource.Token;

    public void Dispose()
    {
        _lifetimeCancellationTokenSource.Cancel();
        _lifetimeCancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}
