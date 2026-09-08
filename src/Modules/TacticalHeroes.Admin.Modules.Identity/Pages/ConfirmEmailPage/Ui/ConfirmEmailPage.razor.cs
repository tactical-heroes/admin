using Microsoft.AspNetCore.Components;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.ConfirmEmailPage.Api;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.ConfirmEmailPage.Ui;

public partial class ConfirmEmailPage(ConfirmEmailApi confirmEmailApi)
{
    private Result? ConfirmationResult { get; set; }

    [SupplyParameterFromQuery]
    public Guid? UserId { get; set; }

    [SupplyParameterFromQuery]
    public string? EmailConfirmationToken { get; set; }

    private bool HasValidParameters =>
        UserId.HasValue && !string.IsNullOrWhiteSpace(EmailConfirmationToken);

    private string? ConfirmationError => ConfirmationResult is { IsFailure: true } result
        ? ApiErrorMessage.FromErrors(result.Errors)
        : null;

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive || !HasValidParameters)
        {
            return;
        }

        ConfirmationResult = await confirmEmailApi.ConfirmEmailAsync(
            UserId!.Value,
            EmailConfirmationToken!,
            LifetimeToken);
    }
}
