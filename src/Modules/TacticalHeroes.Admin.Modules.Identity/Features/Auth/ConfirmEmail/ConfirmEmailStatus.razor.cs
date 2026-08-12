using Microsoft.AspNetCore.Components;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Auth.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Auth.ConfirmEmail;

public partial class ConfirmEmailStatus
{
    private bool _processed;
    private string? _error;

    [Inject]
    private AuthApi AuthApi { get; set; } = null!;

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

        Result result = await AuthApi.ConfirmEmailAsync(
            UserId!.Value,
            EmailConfirmationToken!,
            CancellationToken.None);

        if (result.IsFailure)
        {
            _error = ApiErrorMessage.FromErrors(result.Errors);
        }

        _processed = true;
        StateHasChanged();
    }
}
