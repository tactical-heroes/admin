using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

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
        snackbar,
        navigation)
{
    [PersistentState(AllowUpdates = true)]
    public List<UserStatus>? Statuses { get; set; }

    protected override async Task<Result<UpdateUserLoadState>> LoadCoreAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Task<Result<UpdateUserFormModel>> userTask = updateUserApi.GetAsync(
            id,
            cancellationToken);
        Task<Result<IReadOnlyList<UserStatus>>> statusesTask =
            updateUserApi.GetStatusesAsync(cancellationToken);

        await Task.WhenAll(userTask, statusesTask);

        Result<UpdateUserFormModel> userResult = await userTask;
        Result<IReadOnlyList<UserStatus>> statusesResult = await statusesTask;

        return ResultCombiner.Combine(userResult, statusesResult)
            .Map(static state => new UpdateUserLoadState(state.Item1, state.Item2));
    }

    protected override void ApplyLoadedState(UpdateUserLoadState state)
    {
        Model = state.User;
        Statuses = state.Statuses.ToList();
    }

    protected override void OnLoadStarted()
    {
        Statuses = null;
    }

    protected override Task<Result<Guid>> SaveCoreAsync()
    {
        return updateUserApi.UpdateAsync(Id, Model, LifetimeToken);
    }

    protected override void OnSaveSucceeded(Guid _)
    {
        Snackbar.Add("Пользователь сохранён", Severity.Success);
        Navigation.NavigateTo(IdentityRoutes.Users);
    }

    private string GetStatusDisplayName(string? statusName)
    {
        return Statuses?.FirstOrDefault(status => status.Name == statusName)?.DisplayName
            ?? statusName
            ?? string.Empty;
    }

}
