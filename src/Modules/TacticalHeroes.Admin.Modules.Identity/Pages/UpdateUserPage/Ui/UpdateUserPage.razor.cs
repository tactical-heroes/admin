using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Ui;

public partial class UpdateUserPage(
    UpdateUserApi updateUserApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudUpdateFormComponentBase<
        UpdateUserFormModel,
        UpdateUserFormModelValidator,
        UpdateUserLoadState>(
        updateUserApi.GetStateAsync,
        updateUserApi.UpdateAsync,
        "Пользователь сохранён",
        IdentityRoutes.Users,
        snackbar,
        navigation)
{
    [PersistentState(AllowUpdates = true)]
    public List<UserStatus>? Statuses { get; set; }

    protected override void ApplyLoadedState(UpdateUserLoadState state)
    {
        Model = state.User;
        Statuses = state.Statuses.ToList();
    }

    protected override void OnLoadStarted()
    {
        Statuses = null;
    }

    private string GetStatusDisplayName(string? statusName)
    {
        return Statuses?.FirstOrDefault(status => status.Name == statusName)?.DisplayName
            ?? statusName
            ?? string.Empty;
    }

}
