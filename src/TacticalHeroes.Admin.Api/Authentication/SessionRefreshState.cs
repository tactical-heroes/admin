namespace TacticalHeroes.Admin.Api.Authentication;

public sealed class SessionRefreshState
{
    public const string HeaderName = "X-Session-Refresh-Required";

    private int _isRequired;

    public bool IsRequired => Volatile.Read(ref _isRequired) != 0;

    public event Action? RefreshRequested;

    public void RequireRefresh()
    {
        if (Interlocked.Exchange(ref _isRequired, 1) == 0)
        {
            RefreshRequested?.Invoke();
        }
    }
}
