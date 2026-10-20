using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Shared.Ui.Common;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Ui;

public partial class ClaimsEditor(ClaimCatalogApi api) : CancelableComponentBase
{
    private static readonly (string Code, string Name)[] Actions =
    [
        ("read", "Просмотр"),
        ("create", "Создание"),
        ("update", "Изменение"),
        ("delete", "Удаление")
    ];

    [Parameter, EditorRequired]
    public IList<ClaimValue> Claims { get; set; } = [];

    [PersistentState(AllowUpdates = true)]
    public ClaimCatalog? Permissions { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    private bool IsLoading { get; set; }

    private IEnumerable<IGrouping<string, ClaimOption>> Modules =>
        Permissions?.Items.GroupBy(option => option.ModuleCode) ?? [];

    private int SelectedCount => Permissions?.Items.Count(option => IsSelected(option.Code)) ?? 0;

    protected override Task OnInitializedAsync() =>
        Permissions is null ? LoadAsync() : Task.CompletedTask;

    private async Task LoadAsync()
    {
        IsLoading = true;
        LoadError = null;

        try
        {
            var result = await api.GetPermissionsAsync(LifetimeToken);
            if (result.IsFailure)
            {
                LoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            Permissions = result.Value;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool IsPermission(ClaimValue claim) =>
        string.Equals(claim.Type.Trim(), Permissions?.Type, StringComparison.OrdinalIgnoreCase);

    private bool IsSelected(string code) =>
        Claims.Any(claim => IsPermission(claim) && claim.Value == code);

    private bool? GetSelection(IEnumerable<ClaimOption> permissions)
    {
        var values = permissions.Select(option => IsSelected(option.Code)).ToArray();
        return values.Length > 0 && values.All(value => value)
            ? true
            : values.Any(value => value) ? null : false;
    }

    private void SetPermissions(IEnumerable<ClaimOption> permissions, bool? selected)
    {
        foreach (var option in permissions)
        {
            foreach (var existing in Claims.Where(claim => IsPermission(claim) && claim.Value == option.Code).ToArray())
            {
                Claims.Remove(existing);
            }

            if (selected == true)
            {
                Claims.Add(new ClaimValue { Type = Permissions!.Type, Value = option.Code });
            }
        }
    }

    private static IEnumerable<ClaimOption> ForAction(IEnumerable<ClaimOption> permissions, string action) =>
        permissions.Where(option => option.Code.EndsWith($".{action}", StringComparison.Ordinal));
}
