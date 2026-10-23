using Microsoft.Extensions.DependencyInjection;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.Authentication;

namespace TacticalHeroes.Admin.Client.ComponentTests;

public abstract class ClientComponentTestContext : BunitContext
{
    protected ClientComponentTestContext()
    {
        Services.AddMudServices();
        Services.AddScoped<SessionRefreshState>();
        JSInterop.Mode = JSRuntimeMode.Loose;
        AddAuthorization();
    }
}
