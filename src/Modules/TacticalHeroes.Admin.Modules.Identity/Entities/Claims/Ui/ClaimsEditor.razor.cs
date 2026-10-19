using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Shared.Ui.Common;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Ui;

public partial class ClaimsEditor(ClaimCatalogApi api) : CancelableComponentBase
{
    private ClaimValueValidator Validator { get; } = new();

    [Parameter, EditorRequired]
    public IList<ClaimValue> Claims { get; set; } = [];

    [PersistentState(AllowUpdates = true)]
    public ClaimCatalog? Permissions { get; set; }

    [PersistentState(AllowUpdates = true)]
    public ClaimCatalog? Sections { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    private bool IsLoading { get; set; }

    protected override Task OnInitializedAsync()
    {
        return Permissions is null || Sections is null ? LoadAsync() : Task.CompletedTask;
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        LoadError = null;

        try
        {
            Task<Result<ClaimCatalog>> permissionsTask = api.GetPermissionsAsync(LifetimeToken);
            Task<Result<ClaimCatalog>> sectionsTask = api.GetSectionsAsync(LifetimeToken);
            await Task.WhenAll(permissionsTask, sectionsTask);

            Result<ClaimCatalog> permissions = await permissionsTask;
            Result<ClaimCatalog> sections = await sectionsTask;
            if (permissions.IsFailure || sections.IsFailure)
            {
                LoadError = ApiErrorMessage.FromErrors(
                    permissions.IsFailure ? permissions.Errors : sections.Errors);
                return;
            }

            Permissions = permissions.Value;
            Sections = sections.Value;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private ClaimCatalog? GetCatalog(string type)
    {
        return new[] { Permissions, Sections }.FirstOrDefault(catalog =>
            catalog is not null && string.Equals(catalog.Type, type.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static void ChangeType(ClaimValue claim, string? type)
    {
        claim.Type = type ?? string.Empty;
        claim.Value = string.Empty;
    }

    private static string GetClaimName(ClaimCatalog catalog, string? code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return string.Empty;
        }

        ClaimOption? option = catalog.Items.FirstOrDefault(option => option.Code == code);
        return option is null
            ? $"{code} (нет в каталоге)"
            : option.ResourceDisplayName is null
                ? option.DisplayName
                : $"{option.ResourceDisplayName}: {option.DisplayName}";
    }

    private void AddClaim(string type)
    {
        Claims.Add(new ClaimValue { Type = type });
    }

    private void RemoveClaim(ClaimValue claim)
    {
        Claims.Remove(claim);
    }
}
