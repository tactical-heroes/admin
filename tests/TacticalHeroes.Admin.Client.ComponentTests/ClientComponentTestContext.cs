using MudBlazor.Services;

namespace TacticalHeroes.Admin.Client.ComponentTests;

public abstract class ClientComponentTestContext : BunitContext
{
    protected ClientComponentTestContext()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        AddAuthorization();
    }
}

