using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class DebouncedTextFilter
{
    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    [Parameter, EditorRequired]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? HelperText { get; set; }

    [Parameter]
    public int MinimumLength { get; set; }

    [Parameter]
    public double DebounceInterval { get; set; } = 400;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnFilterChanged { get; set; }

    private Task ApplyFilterAsync(string? value)
    {
        return !string.IsNullOrEmpty(value) && value.Length < MinimumLength
            ? Task.CompletedTask
            : OnFilterChanged.InvokeAsync();
    }
}
