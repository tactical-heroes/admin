using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class AsyncEnumerationSelect<TEnumeration>(
    IEnumerationProvider<TEnumeration> provider)
    : CancelableComponentBase
    where TEnumeration : class, IEnumeration
{
    [PersistentState(AllowUpdates = true)]
    public List<TEnumeration>? Items { get; set; }

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

    protected IReadOnlyCollection<TEnumeration> EnumerationItems => Items ?? [];

    protected override async Task OnInitializedAsync()
    {
        if (Items is not null)
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
            Result<IReadOnlyList<TEnumeration>> result =
                await provider.GetAllAsync(LifetimeToken);

            if (result.IsFailure)
            {
                Items = null;
                LoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            Items = result.Value.ToList();
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
            Items?.FirstOrDefault() is not { } defaultItem)
        {
            return;
        }

        await ValueChanged.InvokeAsync(defaultItem.Name);
    }
}
