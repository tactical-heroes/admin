using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Shared.Ui.Common;

namespace TacticalHeroes.Admin.Client.App.Routing;

public sealed partial class PageLoadTelemetry(NavigationManager navigation) : IDisposable
{
    private PageLoadState _state = new();
    private string? _uri;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnParametersSet()
    {
        if (_uri == navigation.Uri)
        {
            return;
        }
        _state.Changed -= OnStateChanged;
        _uri = navigation.Uri;
        _state = new();
        _state.Changed += OnStateChanged;
    }

    private void OnStateChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _state.Changed -= OnStateChanged;
        GC.SuppressFinalize(this);
    }
}
