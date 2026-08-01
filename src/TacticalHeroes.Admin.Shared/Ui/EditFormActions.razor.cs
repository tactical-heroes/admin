using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class EditFormActions
{
    [Parameter, EditorRequired]
    public string CancelHref { get; set; } = string.Empty;

    [Parameter]
    public bool IsNew { get; set; }

    [Parameter]
    public bool Busy { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnSubmit { get; set; }
}
