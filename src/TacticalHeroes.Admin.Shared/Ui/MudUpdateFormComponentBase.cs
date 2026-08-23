using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Shared.Ui;

public abstract class MudUpdateFormComponentBase<TModel, TValidator>(
    Func<Guid, CancellationToken, Task<Result<TModel>>> loadAsync,
    Func<Guid, TModel, CancellationToken, Task<Result<Guid>>> updateAsync,
    string successMessage,
    string successRoute,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudUpdateFormComponentBase<TModel, TValidator, TModel>(
        loadAsync,
        updateAsync,
        successMessage,
        successRoute,
        snackbar,
        navigation)
    where TModel : class, new()
    where TValidator : MudFormValidator<TModel>, new()
{
    protected sealed override void ApplyLoadedState(TModel state)
    {
        Model = state;
    }
}

public abstract class MudUpdateFormComponentBase<TModel, TValidator, TLoadedState>(
    Func<Guid, CancellationToken, Task<Result<TLoadedState>>> loadAsync,
    Func<Guid, TModel, CancellationToken, Task<Result<Guid>>> updateAsync,
    string successMessage,
    string successRoute,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudFormComponentBase<TModel, TValidator>(
        snackbar,
        navigation,
        successMessage,
        _ => successRoute)
    where TModel : class, new()
    where TValidator : MudFormValidator<TModel>, new()
{
    private long _loadVersion;

    [Parameter]
    public Guid Id { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [PersistentState(AllowUpdates = true)]
    public Guid? LoadedId { get; set; }

    protected bool IsLoading { get; private set; }

    protected sealed override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (LoadedId != Id)
        {
            await ReloadAsync();
        }
    }

    protected async Task ReloadAsync()
    {
        Guid id = Id;
        long loadVersion = ++_loadVersion;

        IsLoading = true;
        LoadError = null;
        LoadedId = id;
        Errors.Clear();

        try
        {
            OnLoadStarted();

            Result<TLoadedState> result = await loadAsync(id, LifetimeToken);

            if (loadVersion != _loadVersion || id != Id)
            {
                return;
            }

            if (result.IsFailure)
            {
                LoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            ApplyLoadedState(result.Value);
        }
        finally
        {
            if (loadVersion == _loadVersion)
            {
                IsLoading = false;
            }
        }
    }

    protected virtual void OnLoadStarted()
    {
    }

    protected abstract void ApplyLoadedState(TLoadedState state);

    protected Task SubmitAsync()
    {
        return base.SubmitAsync(
            cancellationToken => updateAsync(Id, Model, cancellationToken));
    }
}
