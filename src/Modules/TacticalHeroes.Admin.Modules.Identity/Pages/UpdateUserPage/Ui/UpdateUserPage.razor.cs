using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Ui;

public partial class UpdateUserPage(UpdateUserApi updateUserApi)
{
    private bool _loading;

    [Parameter]
    public Guid Id { get; set; }

    [PersistentState(AllowUpdates = true)]
    public List<UserStatus>? Statuses { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [PersistentState(AllowUpdates = true)]
    public Guid? LoadedId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (LoadedId != Id)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        Statuses = null;
        LoadError = null;
        LoadedId = Id;
        Errors.Clear();

        Task<Result<UpdateUserFormModel>> userTask = updateUserApi.GetAsync(
            Id,
            LifetimeToken);
        Task<Result<IReadOnlyList<UserStatus>>> statusesTask =
            updateUserApi.GetStatusesAsync(LifetimeToken);

        await Task.WhenAll(userTask, statusesTask);

        Result<UpdateUserFormModel> userResult = await userTask;
        Result<IReadOnlyList<UserStatus>> statusesResult = await statusesTask;
        Result result = Result.Combine(userResult, statusesResult);

        if (result.IsFailure)
        {
            LoadError = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            Model = userResult.Value;
            Statuses = statusesResult.Value.ToList();
        }

        _loading = false;
    }

    protected override Task<Result> SaveCoreAsync()
    {
        return updateUserApi.UpdateAsync(Id, Model, LifetimeToken);
    }

    protected override void OnSaveSucceeded(Result result)
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
