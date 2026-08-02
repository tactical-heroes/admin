using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.Roles;

public partial class RoleEditPage
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public Guid? Id { get; set; }

    private bool IsNew => !Id.HasValue;

    private string PageTitleText => IsNew
        ? "Новая роль · Tactical Heroes"
        : "Редактирование роли · Tactical Heroes";

    private string HeaderTitle => IsNew
        ? "Новая роль"
        : "Редактирование роли";

    private string HeaderSubtitle => IsNew
        ? "Создайте роль и назначьте ей атрибуты доступа"
        : "Измените название и атрибуты доступа роли";

    private void NavigateToList()
    {
        Navigation.NavigateTo(IdentityRoutes.Roles);
    }
}
