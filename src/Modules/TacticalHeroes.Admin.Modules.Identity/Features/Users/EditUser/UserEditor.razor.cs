using Microsoft.AspNetCore.Components;
using MudBlazor;
using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Users.EditUser;

public partial class UserEditor
{
    private bool _loading;
    private bool _saving;

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

        try
        {
            var userTask = UsersApi.GetAsync(Id.Value);
            var statusesTask = UsersApi.GetStatusesAsync();

            await Task.WhenAll(userTask, statusesTask);

            User = await userTask;
            Statuses = (await statusesTask).ToList();
        }
        catch (Exception exception)
        {
            LoadError = ApiErrorMessage.FromException(exception);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task LoadForCreateAsync()
    {
        _loading = true;
        LoadError = null;

        try
        {
            Statuses = (await UsersApi.GetStatusesAsync()).ToList();
            User = new UserDetails
            {
                Status = Statuses.FirstOrDefault()?.Name ?? string.Empty,
            };
        }
        catch (Exception exception)
        {
            User = null;
            LoadError = ApiErrorMessage.FromException(exception);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task SaveAsync()
    {
        if (User is null)
        {
            return;
        }

        _saving = true;

        try
        {
            if (User.Id == Guid.Empty)
            {
                User.Id = await UsersApi.CreateAsync(User);
                Snackbar.Add("Пользователь создан", Severity.Success);
            }
            else
            {
                await UsersApi.UpdateAsync(User);
                Snackbar.Add("Пользователь сохранён", Severity.Success);
            }

            await Completed.InvokeAsync();
        }
        catch (Exception exception)
        {
            Snackbar.Add(ApiErrorMessage.FromException(exception), Severity.Error);
        }
        finally
        {
            _saving = false;
        }
    }
}
