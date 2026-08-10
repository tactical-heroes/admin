using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Features.Factions.EditFaction;

public partial class FactionEditForm
{
    private MudForm? _form;
    private bool _isValid;

    [Parameter, EditorRequired]
    public FactionDetails Model { get; set; } = new();

    [Parameter]
    public IReadOnlyDictionary<string, string[]> Errors { get; set; } =
        new Dictionary<string, string[]>();

    [Parameter]
    public bool IsNew { get; set; }

    [Parameter]
    public bool Busy { get; set; }

    [Parameter]
    public EventCallback OnSave { get; set; }

    private bool HasError(string field)
    {
        return Errors.ContainsKey(field);
    }

    private string? GetError(string field)
    {
        return Errors.TryGetValue(field, out string[]? messages)
            ? string.Join(" ", messages)
            : null;
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
        {
            return;
        }

        await _form.ValidateAsync();

        if (_isValid)
        {
            await OnSave.InvokeAsync();
        }
    }
}
