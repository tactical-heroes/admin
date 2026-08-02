using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Authentication.ConfirmEmail;

public partial class ConfirmEmailStatus
{
    private bool _processed;
    private string? _error;

    [Inject]
    private AuthenticationApi AuthenticationApi { get; set; } = null!;

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

        try
        {
            await AuthenticationApi.ConfirmEmailAsync(
                UserId!.Value,
                EmailConfirmationToken!);
        }
        catch (Exception exception)
        {
            _error = ApiErrorMessage.FromException(exception);
        }
        finally
        {
            _processed = true;
            StateHasChanged();
        }
    }
}
