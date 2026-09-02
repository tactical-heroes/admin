using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Utilities;

namespace TacticalHeroes.Admin.Modules.Identity.Features.Authentication.Ui;

public partial class AuthenticationFeedback
{
    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public Severity Severity { get; set; } = Severity.Error;

    [Parameter]
    public string? Class { get; set; }

    private string CssClass => new CssBuilder("auth-feedback")
        .AddClass("auth-feedback--error", Severity == Severity.Error)
        .AddClass("auth-feedback--success", Severity == Severity.Success)
        .AddClass(Class)
        .Build();

    private string Role => Severity is Severity.Error or Severity.Warning
        ? "alert"
        : "status";
}
