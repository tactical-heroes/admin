using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.Factions;

public partial class FactionsPage
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "page")]
    public int? PageNumber { get; set; }

    private int CurrentPageNumber => PageNumber is > 0
        ? PageNumber.Value
        : 1;

    private void ChangePage(int pageNumber)
    {
        Navigation.NavigateTo(CompendiumRoutes.FactionsPage(pageNumber));
    }
}
