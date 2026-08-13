using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class PageBackButton
{
    [Parameter, EditorRequired]
    public string Href { get; set; } = string.Empty;
}
