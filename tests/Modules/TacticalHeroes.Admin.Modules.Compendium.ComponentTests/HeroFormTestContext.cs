using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests;

public abstract class HeroFormTestContext : BunitContext
{
    protected static readonly Guid HeroId = Guid.Parse("9d4d9bc8-dc46-4312-a9f2-09dd33936c0b");
    protected static readonly Guid FactionId = Guid.Parse("f341ae7d-69c0-45c6-9a44-110f00127080");

    protected HeroFormHandler Handler { get; } = new();

    protected IRenderedComponent<MudPopoverProvider> Popovers { get; }

    protected HeroFormTestContext()
    {
        Services.AddMudServices();
        Services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"),
            static _ => TimeSpan.FromSeconds(30));
        Services.Configure<HttpClientFactoryOptions>("TacticalHeroesApi",
            options => options.HttpMessageHandlerBuilderActions.Add(
                builder => builder.PrimaryHandler = Handler));
        Services.AddCompendiumAdminModule();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Popovers = Render<MudPopoverProvider>();
    }

    protected async Task FillFormAsync(IRenderedComponent<IComponent> component)
    {
        component.FindComponents<MudTextField<string>>()
            .Single(field => field.Instance.Label == "Имя героя").Find("input").Change("Catherine");
        component.Find("textarea").Change("Leader of the alliance.");
        component.FindComponents<MudNumericField<int>>()
            .Single(field => field.Instance.Label == "Атака").Find("input").Change("12");
        component.FindComponents<MudNumericField<int>>()
            .Single(field => field.Instance.Label == "Защита").Find("input").Change("8");
        component.FindComponents<MudNumericField<int>>()
            .Single(field => field.Instance.Label == "Минимальный урон").Find("input").Change("3");
        component.FindComponents<MudNumericField<int>>()
            .Single(field => field.Instance.Label == "Максимальный урон").Find("input").Change("7");
        component.FindComponent<MudNumericField<double>>().Find("input").Change("1.5");
        component.FindComponents<MudNumericField<int>>()
            .Single(field => field.Instance.Label == "Мораль").Find("input").Change("4");
        component.FindComponents<MudNumericField<int>>()
            .Single(field => field.Instance.Label == "Удача").Find("input").Change("2");
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input("north");
        await Popovers.WaitForAssertionAsync(() => Popovers.Markup.ShouldContain("Northern Alliance"));
        Popovers.FindAll(".mud-list-item")
            .Single(item => item.TextContent.Trim() == "Northern Alliance").Click();
    }

    protected sealed class HeroFormHandler : HttpMessageHandler
    {
        public List<string?> FactionRequests { get; } = [];

        public int FactionDetailRequests { get; private set; }

        public bool EmptyFactions { get; set; }

        public bool FailFactionSearch { get; set; }

        public bool FailFactionLoad { get; set; }

        public TaskCompletionSource<HttpResponseMessage>? PendingFactionSearch { get; set; }

        public CancellationToken FactionSearchCancellation { get; private set; }

        public bool FailHeroLoad { get; set; }

        public int HeroLoads { get; private set; }

        public string? RejectSaveField { get; set; }

        public int Saves { get; private set; }

        public HttpMethod? SaveMethod { get; private set; }

        public JsonElement SavedHero { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string path = request.RequestUri!.AbsolutePath;
            if (path == "/api/v1/factions/select-options")
            {
                request.Method.ShouldBe(HttpMethod.Get);
                if (PendingFactionSearch is not null)
                {
                    FactionSearchCancellation = cancellationToken;
                    return await PendingFactionSearch.Task.WaitAsync(cancellationToken);
                }

                return GetFactions(request.RequestUri);
            }

            if (path == "/api/v1/factions/" + FactionId)
            {
                FactionDetailRequests++;
                return FailFactionLoad
                    ? JsonResponse(HttpStatusCode.BadRequest, new { status = 400, detail = "Faction unavailable." })
                    : JsonResponse(HttpStatusCode.OK, new { id = FactionId, name = "Northern Alliance", description = "A faction." });
            }

            if (request.Method == HttpMethod.Get)
            {
                path.ShouldBe("/api/v1/heroes/" + HeroId);
                HeroLoads++;
                return FailHeroLoad
                    ? JsonResponse(HttpStatusCode.BadRequest, new { status = 400, detail = "Hero unavailable." })
                    : JsonResponse(HttpStatusCode.OK, new
                    {
                        id = HeroId,
                        name = "Catherine",
                        description = "Leader of the alliance.",
                        factionId = FactionId,
                        attack = 12,
                        defense = 8,
                        minimumDamage = 3,
                        maximumDamage = 7,
                        initiative = 1.5,
                        morale = 4,
                        luck = 2
                    });
            }

            path.ShouldBe(request.Method == HttpMethod.Post
                ? "/api/v1/heroes"
                : "/api/v1/heroes/" + HeroId);
            Saves++;
            SaveMethod = request.Method;
            SavedHero = JsonSerializer.Deserialize<JsonElement>(
                await request.Content!.ReadAsStringAsync(cancellationToken));
            if (RejectSaveField is not null)
            {
                return JsonResponse(HttpStatusCode.BadRequest, new
                {
                    status = 400,
                    errors = new Dictionary<string, string[]> { [RejectSaveField] = ["Server rejected value."] }
                });
            }

            return request.Method == HttpMethod.Post
                ? JsonResponse(HttpStatusCode.Created, new { id = HeroId })
                : new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        private HttpResponseMessage GetFactions(Uri uri)
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            query["limit"].ShouldBe("20");
            string? search = query["search"];
            FactionRequests.Add(search);
            if (FailFactionSearch)
            {
                return JsonResponse(HttpStatusCode.BadRequest, new { status = 400, detail = "Factions unavailable." });
            }

            var options = new[]
            {
                new { id = new Guid(1, 0, 0, new byte[8]), name = "Faction 1" },
                new { id = FactionId, name = "Northern Alliance" }
            };

            return JsonResponse(HttpStatusCode.OK, EmptyFactions
                ? []
                : options.Where(option => string.IsNullOrWhiteSpace(search)
                    || option.name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray());
        }

        private static HttpResponseMessage JsonResponse(HttpStatusCode status, object body)
        {
            return new HttpResponseMessage(status) { Content = JsonContent.Create(body) };
        }
    }
}
