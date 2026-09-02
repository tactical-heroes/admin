using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

public partial class ResendConfirmationForm(LoginApi loginApi)
{
    [Parameter]
    public string? ReturnUrl { get; set; }

    private string LoginHref => IdentityRoutes.LoginPage(ReturnUrl);

    private Task SubmitAsync()
    {
        return SubmitResultAsync(cancellationToken =>
            loginApi.ResendConfirmationEmailAsync(
                Model.Email,
                cancellationToken));
    }
}
