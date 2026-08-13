using Microsoft.AspNetCore.Components;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.ConfirmEmailPage.Api;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.ConfirmEmailPage.Ui;

public partial class ConfirmEmailPage
{
    private bool _processed;
    private string? _error;

    [Inject]
    private ConfirmEmailApi ConfirmEmailApi { get; set; } = null!;

    [Parameter]
    public Guid? UserId { get; set; }

    [Parameter]
    public string? EmailConfirmationToken { get; set; }

    private bool HasValidParameters =>
        UserId.HasValue && !string.IsNullOrWhiteSpace(EmailConfirmationToken);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !HasValidParameters)
        {
            return;
        }

        Result result = await ConfirmEmailApi.ConfirmEmailAsync(
            UserId!.Value,
            EmailConfirmationToken!,
            LifetimeToken);

        if (result.IsFailure)
        {
            _error = ApiErrorMessage.FromErrors(result.Errors);
        }

        _processed = true;
        StateHasChanged();
    }
}
