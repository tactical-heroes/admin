using Common.SearchParams.Core;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Pl.Ui.Blazor.Services.Interfaces.Core;

namespace Pl.Ui.Blazor.Admin.Pages.Core;

public abstract class BaseEdit<TId, TViewModel, TSearchParams, TConvertParams, TService> : ComponentBase
    where TId : struct
    where TViewModel : class, new()
    where TSearchParams : BaseSearchParams, new()
    where TConvertParams : class, new()
    where TService : IBaseService<TId, TViewModel, TSearchParams, TConvertParams>
{
    [Inject] protected TService Service { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    [Parameter] public TId? Id { get; set; }

    protected abstract string TableRoute { get; }
    protected abstract string EditRoute { get; }

    protected readonly IMask PhoneMask = new PatternMask("(000) 000-00-00");

    protected TViewModel _model = new();
    protected static readonly Type ModelType = typeof(TViewModel);

    protected bool _loading;
    protected bool _saving;
    protected string? _error;

    protected bool IsEditMode => Id.HasValue;
    protected string Title => IsEditMode ? "Редактирование" : "Создание";

    protected override async Task OnParametersSetAsync()
    {
        _error = null;

        if (!IsEditMode)
        {
            return;
        }

        _loading = true;

        try
        {
            _model = await Service.GetAsync(Id!.Value);
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    protected virtual async Task OnValidSubmit()
    {
        _saving = true;
        _error = null;

        try
        {
            if (IsEditMode)
            {
                await Service.UpdateAsync(Id!.Value, _model, CancellationToken.None);
                Snackbar.Add("Успешно.", Severity.Success);
            }
            else
            {
                var createdId = await Service.CreateAsync(_model, CancellationToken.None);
                Navigation.NavigateTo($"{EditRoute}/{createdId}");
                Snackbar.Add("Успешно.", Severity.Success);
            }
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _saving = false;
        }
    }

    protected void OnCancelClicked()
    {
        Navigation.NavigateTo(TableRoute);
    }
}
