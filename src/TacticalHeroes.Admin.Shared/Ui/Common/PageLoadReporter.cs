using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui.Common;

public sealed class PageLoadReporter : ComponentBase, IDisposable
{
    private PageLoadState? _registeredState;

    [CascadingParameter]
    public PageLoadState? State { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public bool HasError { get; set; }

    protected override void OnParametersSet()
    {
        if (_registeredState != State)
        {
            _registeredState?.Remove(this);
            _registeredState = State;
        }
        _registeredState?.Report(this, Loading, HasError);
    }

    public void Dispose()
    {
        _registeredState?.Remove(this);
    }
}
