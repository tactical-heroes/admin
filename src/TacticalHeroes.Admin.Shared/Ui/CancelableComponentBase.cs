using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public abstract class CancelableComponentBase : ComponentBase, IDisposable
{
    private readonly CancellationTokenSource _lifetimeCancellationTokenSource = new();
    private bool _disposed;

    protected CancellationToken LifetimeToken => _lifetimeCancellationTokenSource.Token;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }

        _lifetimeCancellationTokenSource.Cancel();
        _lifetimeCancellationTokenSource.Dispose();
        _disposed = true;
    }
}
