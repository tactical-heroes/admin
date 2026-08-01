using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.Factions;

public partial class FactionEditPage
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public Guid? Id { get; set; }

    private bool IsNew => !Id.HasValue;

    private string PageTitleText => IsNew
        ? "Новая фракция · Tactical Heroes"
        : "Редактирование фракции · Tactical Heroes";

    private string HeaderTitle => IsNew
        ? "Новая фракция"
        : "Редактирование фракции";

    private string HeaderSubtitle => IsNew
        ? "Добавьте фракцию в Compendium"
        : "Измените название и описание фракции";

    private void NavigateToList()
    {
        Navigation.NavigateTo(CompendiumRoutes.Factions);
    }
}
