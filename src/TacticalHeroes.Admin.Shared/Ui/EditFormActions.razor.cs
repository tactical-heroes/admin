using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class EditFormActions
{
    private string ActionText => IsNew ? "Создать" : "Сохранить";

    private string ActionIcon => IsNew
        ? Icons.Material.Filled.Add
        : Icons.Material.Filled.Save;

    [Parameter, EditorRequired]
    public string CancelHref { get; set; } = string.Empty;

    [Parameter]
    public bool IsNew { get; set; }

    [Parameter]
    public bool Busy { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnSubmit { get; set; }
}
