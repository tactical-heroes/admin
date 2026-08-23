using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Ui;

public partial class UpdateUserPage(
    UpdateUserApi updateUserApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudUpdateFormComponentBase<UpdateUserFormModel, UpdateUserFormModelValidator>(
        updateUserApi.GetAsync,
        updateUserApi.UpdateAsync,
        "Пользователь сохранён",
        IdentityRoutes.Users,
        snackbar,
        navigation)
{
}
