using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class EnumerationSelect
{
    [Parameter, EditorRequired]
    public IReadOnlyCollection<IEnumeration> Items { get; set; } = [];

    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<string>>? For { get; set; }

    [Parameter, EditorRequired]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public Variant Variant { get; set; } = Variant.Outlined;

    [Parameter]
    public Margin Margin { get; set; } = Margin.None;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public string? ErrorText { get; set; }

    private string GetDisplayName(string? name)
    {
        return Items.FirstOrDefault(item => item.Name == name)?.DisplayName
            ?? name
            ?? string.Empty;
    }
}
