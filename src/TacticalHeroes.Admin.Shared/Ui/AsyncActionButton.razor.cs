using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class AsyncActionButton
{
    [Parameter, EditorRequired]
    public string Text { get; set; } = string.Empty;

    [Parameter]
    public string? BusyText { get; set; }

    [Parameter]
    public bool Busy { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ShowProgress { get; set; } = true;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public Variant Variant { get; set; } = Variant.Filled;

    [Parameter]
    public Color Color { get; set; } = Color.Primary;

    [Parameter]
    public string? StartIcon { get; set; }

    [Parameter]
    public ButtonType ButtonType { get; set; } = ButtonType.Button;

    [Parameter, EditorRequired]
    public EventCallback OnClick { get; set; }

    private string AriaBusy => Busy ? "true" : "false";

    private string DisplayText => Busy ? BusyText ?? Text : Text;
}
