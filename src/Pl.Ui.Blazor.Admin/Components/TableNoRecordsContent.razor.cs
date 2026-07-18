using Microsoft.AspNetCore.Components;

namespace Pl.Ui.Blazor.Admin.Components;

public partial class TableNoRecordsContent
{
    [Parameter] public string Text { get; set; } = "Ничего не найдено";
}
