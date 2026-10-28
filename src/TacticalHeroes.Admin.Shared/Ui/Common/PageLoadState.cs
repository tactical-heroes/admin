namespace TacticalHeroes.Admin.Shared.Ui.Common;

public sealed class PageLoadState
{
    private readonly Dictionary<object, (bool Loading, bool HasError)> _loaders = [];

    public event Action? Changed;

    public bool IsLoading => _loaders.Values.Any(state => state.Loading);

    public bool HasError => _loaders.Values.Any(state => state.HasError);

    internal void Report(object loader, bool loading, bool hasError)
    {
        var state = (loading, hasError);
        if (_loaders.TryGetValue(loader, out var previous) && previous == state)
        {
            return;
        }
        _loaders[loader] = state;
        Changed?.Invoke();
    }

    internal void Remove(object loader)
    {
        if (_loaders.Remove(loader))
        {
            Changed?.Invoke();
        }
    }
}
