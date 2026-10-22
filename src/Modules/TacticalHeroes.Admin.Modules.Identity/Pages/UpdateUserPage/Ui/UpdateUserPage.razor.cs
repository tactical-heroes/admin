using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Ui;

public partial class UpdateUserPage(
    UpdateUserApi updateUserApi,
    ISnackbar snackbar,
    NavigationManager navigation,
    AuthenticationStateProvider authenticationStateProvider)
    : MudUpdateFormComponentBase<UpdateUserFormModel, UpdateUserFormModelValidator>(
        updateUserApi.GetAsync,
        updateUserApi.UpdateAsync,
        "Пользователь сохранён",
        IdentityRoutes.Users,
        snackbar,
        navigation)
{
    protected override async Task NavigateAfterSaveAsync(Guid id)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (Guid.TryParse(state.User.FindFirst("sub")?.Value, out var userId) && userId == id)
        {
            Navigation.NavigateTo(IdentityRoutes.AuthenticationRefresh, forceLoad: true);
            return;
        }

        await base.NavigateAfterSaveAsync(id);
    }
}
