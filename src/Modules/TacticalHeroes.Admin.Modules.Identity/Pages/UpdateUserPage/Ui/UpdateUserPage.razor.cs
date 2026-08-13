using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Ui;

public partial class UpdateUserPage
{
    private readonly FormErrorState<UpdateUserFormModel> _errors = new();
    private readonly UpdateUserFormModelValidator _validator = new();
    private bool _loading;

    [Inject]
    private UpdateUserApi UpdateUserApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    [PersistentState(AllowUpdates = true)]
    public UpdateUserFormModel? User { get; set; }

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
        User = null;
        Statuses = null;
        LoadError = null;
        LoadedId = Id;
        _errors.Clear();

        Task<Result<UpdateUserFormModel>> userTask = UpdateUserApi.GetAsync(
            Id,
            LifetimeToken);
        Task<Result<IReadOnlyList<UserStatus>>> statusesTask =
            UpdateUserApi.GetStatusesAsync(LifetimeToken);

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
            User = userResult.Value;
            Statuses = statusesResult.Value.ToList();
        }

        _loading = false;
    }

    protected override async Task SaveAsync()
    {
        if (User is null)
        {
            return;
        }

        _errors.Clear();

        Result result = await UpdateUserApi.UpdateAsync(Id, User, LifetimeToken);

        if (result.IsFailure)
        {
            _errors.Handle(result.Errors, Snackbar);
            return;
        }

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
