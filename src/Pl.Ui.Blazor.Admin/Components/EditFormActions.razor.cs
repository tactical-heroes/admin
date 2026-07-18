using Microsoft.AspNetCore.Components;

namespace Pl.Ui.Blazor.Admin.Components;

public partial class EditFormActions
{
    [Parameter, EditorRequired] public EventCallback OnCancel { get; set; }

    [Parameter] public bool IsEditMode { get; set; }
    [Parameter] public bool Disabled { get; set; }

    [Parameter] public string CancelText { get; set; } = "Отмена";
    [Parameter] public string CreateText { get; set; } = "Создать";
    [Parameter] public string SaveText { get; set; } = "Сохранить";

    [Parameter] public string Class { get; set; } = "mt-6";

    protected string SubmitText
    {
        get { return IsEditMode ? SaveText : CreateText; }
    }
}
