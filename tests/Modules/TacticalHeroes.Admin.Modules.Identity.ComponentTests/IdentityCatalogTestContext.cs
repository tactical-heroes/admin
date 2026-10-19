using System.Net;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests;

public abstract class IdentityCatalogTestContext : BunitContext
{
    protected CatalogHandler Handler { get; } = new();

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
        JSInterop.Mode = JSRuntimeMode.Loose;
        Render<MudPopoverProvider>();
    }

    protected sealed class CatalogHandler : HttpMessageHandler
    {
        public bool Fail { get; set; }

        public bool MultipleRolePages { get; set; }

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

            if (MultipleRolePages && request.RequestUri!.AbsolutePath == "/api/v1/roles")
            {
                bool secondPage = request.RequestUri.Query.Contains("pageNumber=2", StringComparison.Ordinal);
                string id = secondPage ? "b4e775b5-ff1e-4fe5-ad27-b0f09d7b83f1" : IdentityCatalogResponses.RoleId.ToString();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent($$"""
                        {"items":[{"id":"{{id}}","name":"Role {{(secondPage ? 2 : 1)}}","isDefault":false}],
                         "pageNumber":{{(secondPage ? 2 : 1)}},"pageSize":100,"totalPages":2,"totalCount":101}
                        """, Encoding.UTF8, "application/json"),
                });
            }

            return Task.FromResult(IdentityCatalogResponses.Create(request)
                ?? new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
