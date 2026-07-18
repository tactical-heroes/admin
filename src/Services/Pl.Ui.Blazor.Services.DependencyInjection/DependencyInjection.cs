using Microsoft.Extensions.DependencyInjection;

using Pl.Ui.Blazor.Services.Implementations;
using Pl.Ui.Blazor.Services.Interfaces;

namespace Pl.Ui.Blazor.Services.DependencyInjection;

public static class DependencyInjection
{
    public static void UseServices(this IServiceCollection services)
    {
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IAvatarsService, AvatarsService>();
    }
}
