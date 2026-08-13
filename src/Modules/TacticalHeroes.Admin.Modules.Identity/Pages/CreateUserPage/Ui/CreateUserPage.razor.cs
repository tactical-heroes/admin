using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Api;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Ui;

public partial class CreateUserPage
{
    private bool _loading;

    [Inject]
    private CreateUserApi CreateUserApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

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
            await CreateUserApi.GetStatusesAsync(LifetimeToken);

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

    protected override Task ExecuteSaveAsync()
    {
        return SaveAsync(
            () => CreateUserApi.CreateAsync(Model, LifetimeToken),
            id =>
            {
                Snackbar.Add("Пользователь создан", Severity.Success);
                Navigation.NavigateTo(IdentityRoutes.User(id));
            });
    }

    private string GetStatusDisplayName(string? statusName)
    {
        return Statuses?.FirstOrDefault(status => status.Name == statusName)?.DisplayName
            ?? statusName
            ?? string.Empty;
    }

}
