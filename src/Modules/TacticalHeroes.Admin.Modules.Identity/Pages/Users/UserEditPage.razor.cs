using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.Users;

public partial class UserEditPage
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public Guid? Id { get; set; }

    private bool IsNew => !Id.HasValue;

    private string PageTitleText => IsNew
        ? "Новый пользователь · Tactical Heroes"
        : "Редактирование пользователя · Tactical Heroes";

    private string HeaderTitle => IsNew
        ? "Новый пользователь"
        : "Редактирование пользователя";

    private string HeaderSubtitle => IsNew
        ? "Создайте аккаунт и настройте доступ пользователя"
        : "Измените профиль, статус и атрибуты доступа пользователя";

    private void NavigateToList()
    {
        Navigation.NavigateTo(IdentityRoutes.Users);
    }
}
