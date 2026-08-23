using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Ui;

public partial class CreateUserPage(
    CreateUserApi createUserApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudCreateFormComponentBase<CreateUserFormModel, CreateUserFormModelValidator>(
        createUserApi.CreateAsync,
        "Пользователь создан",
        IdentityRoutes.User,
        snackbar,
        navigation)
{
}
