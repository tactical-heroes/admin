using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class LoadableContent
{
    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public string? LoadError { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnRetry { get; set; }

    [Parameter, EditorRequired]
    public RenderFragment LoadingContent { get; set; } = null!;

    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;
}
