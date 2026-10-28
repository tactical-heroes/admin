using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Shared.Ui.Common;

namespace TacticalHeroes.Admin.Client.App.Routing;

public partial class PageLoadTelemetry : IDisposable
{
    private readonly PageLoadState _state = new();

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        _state.Changed += OnStateChanged;
    }

    private void OnStateChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _state.Changed -= OnStateChanged;
    }
}
