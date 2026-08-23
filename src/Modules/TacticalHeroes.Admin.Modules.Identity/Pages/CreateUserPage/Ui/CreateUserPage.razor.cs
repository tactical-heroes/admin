using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Model;
using TacticalHeroes.Admin.Shared.Errors;
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
    private bool _loading;

    [PersistentState(AllowUpdates = true)]
    public List<UserStatus>? Statuses { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Statuses is null)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        LoadError = null;
        Errors.Clear();

        Result<IReadOnlyList<UserStatus>> result =
            await createUserApi.GetStatusesAsync(LifetimeToken);

        if (result.IsFailure)
        {
            Statuses = null;
            LoadError = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            Statuses = result.Value.ToList();
            Model.Status = Statuses.FirstOrDefault()?.Name ?? string.Empty;
        }

        _loading = false;
    }

    private string GetStatusDisplayName(string? statusName)
    {
        return Statuses?.FirstOrDefault(status => status.Name == statusName)?.DisplayName
            ?? statusName
            ?? string.Empty;
    }

}
