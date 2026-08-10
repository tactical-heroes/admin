using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Users.EditUser;

public partial class UserEditor
{
    private bool _loading;
    private bool _saving;
    private IReadOnlyDictionary<string, string[]> _fieldErrors =
        new Dictionary<string, string[]>();

    [Inject]
    private UsersApi UsersApi { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public Guid? Id { get; set; }

    [Parameter]
    public EventCallback Completed { get; set; }

    [PersistentState]
    public UserDetails? User { get; set; }

    [PersistentState]
    public List<UserStatus>? Statuses { get; set; }

    [PersistentState]
    public string? LoadError { get; set; }

    private bool IsNew => !Id.HasValue;

    protected override async Task OnParametersSetAsync()
    {
        if (!Id.HasValue)
        {
            if (User is null || User.Id != Guid.Empty || Statuses is null)
            {
                await LoadForCreateAsync();
            }

            return;
        }

        if (User?.Id != Id.Value || Statuses is null)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        if (!Id.HasValue)
        {
            return;
        }

        _loading = true;
        LoadError = null;
        _fieldErrors = new Dictionary<string, string[]>();

        Task<Result<UserDetails>> userTask = UsersApi.GetAsync(Id.Value);
        Task<Result<IReadOnlyList<UserStatus>>> statusesTask = UsersApi.GetStatusesAsync();

        await Task.WhenAll(userTask, statusesTask);

        Result<UserDetails> userResult = await userTask;
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

    private async Task LoadForCreateAsync()
    {
        _loading = true;
        LoadError = null;
        _fieldErrors = new Dictionary<string, string[]>();

        Result<IReadOnlyList<UserStatus>> result = await UsersApi.GetStatusesAsync();

        if (result.IsFailure)
        {
            User = null;
            LoadError = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            Statuses = result.Value.ToList();
            User = new UserDetails
            {
                Status = Statuses.FirstOrDefault()?.Name ?? string.Empty,
            };
        }

        _loading = false;
    }

    private async Task SaveAsync()
    {
        if (User is null)
        {
            return;
        }

        _saving = true;
        _fieldErrors = new Dictionary<string, string[]>();

        if (User.Id == Guid.Empty)
        {
            Result<Guid> result = await UsersApi.CreateAsync(User);

            if (result.IsFailure)
            {
                HandleErrors(result.Errors);
                _saving = false;
                return;
            }

            User.Id = result.Value;
            Snackbar.Add("Пользователь создан", Severity.Success);
        }
        else
        {
            Result result = await UsersApi.UpdateAsync(User);

            if (result.IsFailure)
            {
                HandleErrors(result.Errors);
                _saving = false;
                return;
            }

            Snackbar.Add("Пользователь сохранён", Severity.Success);
        }

        _saving = false;
        await Completed.InvokeAsync();
    }

    private void HandleErrors(IReadOnlyList<Error> errors)
    {
        _fieldErrors = ApiErrorMessage.GetFieldErrors(errors, MapField);
        IReadOnlyList<Error> unhandledErrors =
            ApiErrorMessage.GetUnhandledErrors(errors, MapField);

        if (unhandledErrors.Count > 0)
        {
            Snackbar.Add(ApiErrorMessage.FromErrors(unhandledErrors), Severity.Error);
        }
    }

    private static string? MapField(string field)
    {
        if (string.Equals(field, nameof(UserDetails.Email), StringComparison.OrdinalIgnoreCase))
        {
            return nameof(UserDetails.Email);
        }

        if (string.Equals(field, nameof(UserDetails.UserName), StringComparison.OrdinalIgnoreCase))
        {
            return nameof(UserDetails.UserName);
        }

        if (string.Equals(field, nameof(UserDetails.Password), StringComparison.OrdinalIgnoreCase))
        {
            return nameof(UserDetails.Password);
        }

        if (string.Equals(field, nameof(UserDetails.Status), StringComparison.OrdinalIgnoreCase)
            || string.Equals(field, "UserStatus", StringComparison.OrdinalIgnoreCase))
        {
            return nameof(UserDetails.Status);
        }

        return null;
    }
}
