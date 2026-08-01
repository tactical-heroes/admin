using Microsoft.AspNetCore.Components;
using MudBlazor;
using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Features.Factions.EditFaction;

public partial class FactionEditor
{
    private bool _loading;
    private bool _saving;
    private bool _deleting;

    [Inject]
    private FactionsApi FactionsApi { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public Guid? Id { get; set; }

    [Parameter]
    public EventCallback Completed { get; set; }

    [PersistentState]
    public FactionDetails? Faction { get; set; }

    [PersistentState]
    public string? LoadError { get; set; }

    private bool IsNew => !Id.HasValue;

    protected override async Task OnParametersSetAsync()
    {
        if (!Id.HasValue)
        {
            if (Faction is null || Faction.Id.HasValue)
            {
                Faction = new FactionDetails();
            }

            LoadError = null;
            return;
        }

        if (Faction?.Id != Id.Value)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        if (!Id.HasValue)
        {
            return;
        }

        _loading = true;
        LoadError = null;

        try
        {
            Faction = await FactionsApi.GetAsync(Id.Value);
        }
        catch (Exception exception)
        {
            LoadError = ApiErrorMessage.FromException(exception);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task SaveAsync()
    {
        if (Faction is null)
        {
            return;
        }

        _saving = true;

        try
        {
            if (Faction.Id.HasValue)
            {
                await FactionsApi.UpdateAsync(Faction);
                Snackbar.Add("Фракция сохранена", Severity.Success);
            }
            else
            {
                Faction.Id = await FactionsApi.CreateAsync(Faction);
                Snackbar.Add("Фракция создана", Severity.Success);
            }

            await Completed.InvokeAsync();
        }
        catch (Exception exception)
        {
            Snackbar.Add(ApiErrorMessage.FromException(exception), Severity.Error);
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task DeleteAsync()
    {
        if (Faction?.Id is not Guid id)
        {
            return;
        }

        _deleting = true;

        try
        {
            await FactionsApi.DeleteAsync(id);
            Snackbar.Add("Фракция удалена", Severity.Success);
            await Completed.InvokeAsync();
        }
        catch (Exception exception)
        {
            Snackbar.Add(ApiErrorMessage.FromException(exception), Severity.Error);
        }
        finally
        {
            _deleting = false;
        }
    }
}
