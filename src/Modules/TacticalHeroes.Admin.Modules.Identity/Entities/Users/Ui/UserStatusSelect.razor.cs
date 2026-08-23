using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Users.Ui;

public partial class UserStatusSelect(UserStatusApi userStatusApi)
    : CancelableComponentBase
{
    [PersistentState(AllowUpdates = true)]
    public List<UserStatus>? Statuses { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<string>>? For { get; set; }

    [Parameter, EditorRequired]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public Variant Variant { get; set; } = Variant.Outlined;

    [Parameter]
    public Margin Margin { get; set; } = Margin.None;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public string? ErrorText { get; set; }

    [Parameter]
    public bool UseFirstAsDefault { get; set; }

    protected bool IsLoading { get; private set; }

    protected IReadOnlyCollection<UserStatus> Items => Statuses ?? [];

    protected override async Task OnInitializedAsync()
    {
        if (Statuses is not null)
        {
            await ApplyDefaultValueAsync();
            return;
        }

        await LoadAsync();
    }

    protected async Task LoadAsync()
    {
        IsLoading = true;
        LoadError = null;

        try
        {
            Result<IReadOnlyList<UserStatus>> result =
                await userStatusApi.GetAllAsync(LifetimeToken);

            if (result.IsFailure)
            {
                Statuses = null;
                LoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            Statuses = result.Value.ToList();
            await ApplyDefaultValueAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ApplyDefaultValueAsync()
    {
        if (!UseFirstAsDefault ||
            !string.IsNullOrEmpty(Value) ||
            Statuses?.FirstOrDefault() is not { } defaultStatus)
        {
            return;
        }

        await ValueChanged.InvokeAsync(defaultStatus.Name);
    }
}
