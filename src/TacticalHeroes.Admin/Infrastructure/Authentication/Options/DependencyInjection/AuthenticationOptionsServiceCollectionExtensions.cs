using Microsoft.Extensions.Options;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Options.DependencyInjection;

internal static class AuthenticationOptionsServiceCollectionExtensions
{
    internal static IServiceCollection AddAdminAuthenticationOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<
            IValidateOptions<AdminSessionOptions>,
            AdminSessionOptionsValidator>();
        services
            .AddOptions<AdminSessionOptions>()
            .Bind(configuration.GetRequiredSection(AdminSessionOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<
            IValidateOptions<AdminOpenIdConnectOptions>,
            AdminOpenIdConnectOptionsValidator>();
        services
            .AddOptions<AdminOpenIdConnectOptions>()
            .Bind(configuration.GetRequiredSection(AdminOpenIdConnectOptions.SectionName))
            .ValidateOnStart();

        return services;
    }
}
