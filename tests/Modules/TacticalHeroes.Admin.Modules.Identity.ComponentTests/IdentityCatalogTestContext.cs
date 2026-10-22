using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Shared.Navigation;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests;

public abstract class IdentityCatalogTestContext : BunitContext
{
    protected CatalogHandler Handler { get; } = new();

    protected List<AdminNavigationGroup> NavigationGroups { get; } = [];

    protected IRenderedComponent<MudPopoverProvider> Popovers { get; }

    protected IdentityCatalogTestContext()
    {
        Services.AddMudServices();
        Services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"),
            static _ => TimeSpan.FromSeconds(30));
        Services.Configure<HttpClientFactoryOptions>("TacticalHeroesApi",
            options => options.HttpMessageHandlerBuilderActions.Add(
                builder => builder.PrimaryHandler = Handler));
        Services.AddIdentityAdminModule();
        Services.AddSingleton<IEnumerable<AdminNavigationGroup>>(NavigationGroups);
        JSInterop.Mode = JSRuntimeMode.Loose;
        Popovers = Render<MudPopoverProvider>();
    }

    protected sealed class CatalogHandler : HttpMessageHandler
    {
        public bool Fail { get; set; }

        public IReadOnlyList<RoleOption>? Roles { get; set; }

        public ClaimCatalog? Permissions { get; set; }

        public List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            if (Fail)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("""{"detail":"Catalog unavailable"}""", Encoding.UTF8, "application/json"),
                });
            }

            if (Roles is not null && request.RequestUri!.AbsolutePath == "/api/v1/roles/select-options")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(Roles, JsonSerializerOptions.Web), Encoding.UTF8, "application/json")
                });
            }

            if (Permissions is not null && request.RequestUri!.AbsolutePath == "/api/v1/claims/permissions")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(Permissions, JsonSerializerOptions.Web), Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(IdentityCatalogResponses.Create(request)
                ?? new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
