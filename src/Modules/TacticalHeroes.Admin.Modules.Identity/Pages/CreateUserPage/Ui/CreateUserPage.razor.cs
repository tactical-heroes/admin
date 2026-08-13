using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Ui;

public partial class CreateUserPage
{
    private readonly FormErrorState<CreateUserFormModel> _errors = new();
    private MudForm? _form;
    private bool _isValid;
    private bool _loading;
    private bool _saving;

    [Inject]
    private UsersApi UsersApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [PersistentState(AllowUpdates = true)]
    public CreateUserFormModel? PersistedUser { get; set; }

    [PersistentState(AllowUpdates = true)]
    public List<UserStatus>? Statuses { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    private CreateUserFormModel User => PersistedUser ??= new();

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
        _errors.Clear();

        Result<IReadOnlyList<UserStatus>> result =
            await UsersApi.GetStatusesAsync(CancellationToken.None);

        if (result.IsFailure)
        {
            Statuses = null;
            LoadError = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            Statuses = result.Value.ToList();
            User.Status = Statuses.FirstOrDefault()?.Name ?? string.Empty;
        }

        _loading = false;
    }

    private async Task SaveAsync()
    {
        _saving = true;
        _errors.Clear();

        Result<Guid> result = await UsersApi.CreateAsync(User, CancellationToken.None);

        if (result.IsFailure)
        {
            _errors.Handle(result.Errors, Snackbar);
            _saving = false;
            return;
        }

        Snackbar.Add("Пользователь создан", Severity.Success);
        Navigation.NavigateTo(IdentityRoutes.User(result.Value));
    }

    private string GetStatusDisplayName(string? statusName)
    {
        return Statuses?.FirstOrDefault(status => status.Name == statusName)?.DisplayName
            ?? statusName
            ?? string.Empty;
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
        {
            return;
        }

        await _form.ValidateAsync();

        if (_isValid)
        {
            await SaveAsync();
        }
    }
}
