using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests;

public abstract class UnitFormTestContext : BunitContext
{
    protected static readonly Guid UnitId = Guid.Parse("9d4d9bc8-dc46-4312-a9f2-09dd33936c0b");
    protected static readonly Guid FactionId = Guid.Parse("f341ae7d-69c0-45c6-9a44-110f00127080");

    protected UnitFormHandler Handler { get; } = new();

    protected IRenderedComponent<MudPopoverProvider> Popovers { get; }

    protected UnitFormTestContext()
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
            .Single(field => field.Instance.Label == "Имя юнита").Find("input").Change("Archer");
        component.Find("textarea").Change("Ranged unit of the alliance.");
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
        component.FindComponents<MudNumericField<int>>()
            .Single(field => field.Instance.Label == "Здоровье").Find("input").Change("25");
        component.FindComponents<MudNumericField<int>>()
            .Single(field => field.Instance.Label == "Скорость").Find("input").Change("6");
        component.FindComponents<MudNumericField<int?>>()
            .Single(field => field.Instance.Label == "Количество выстрелов").Find("input").Change("12");
        component.FindComponents<MudNumericField<int?>>()
            .Single(field => field.Instance.Label == "Дальность стрельбы").Find("input").Change("8");
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input("north");
        await Popovers.WaitForAssertionAsync(() => Popovers.Markup.ShouldContain("Northern Alliance"));
        await Popovers.FindAll(".mud-list-item")
            .Single(item => item.TextContent.Trim() == "Northern Alliance").ClickAsync(new MouseEventArgs());
    }

    protected sealed class UnitFormHandler : HttpMessageHandler
    {
        public List<string?> FactionRequests { get; } = [];

        public bool EmptyFactions { get; set; }

        public bool FailUnitLoad { get; set; }

        public bool Ranged { get; set; } = true;

        public int UnitLoads { get; private set; }

        public string? RejectSaveField { get; set; }

        public int Saves { get; private set; }

        public HttpMethod? SaveMethod { get; private set; }

        public JsonElement SavedUnit { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string path = request.RequestUri!.AbsolutePath;
            if (path == "/api/v1/factions/select-options")
            {
                request.Method.ShouldBe(HttpMethod.Get);
                return GetFactions(request.RequestUri);
            }

            if (request.Method == HttpMethod.Get)
            {
                path.ShouldBe("/api/v1/units/" + UnitId);
                UnitLoads++;
                return FailUnitLoad
                    ? JsonResponse(HttpStatusCode.BadRequest, new { status = 400, detail = "Unit unavailable." })
                    : JsonResponse(HttpStatusCode.OK, new
                    {
                        id = UnitId,
                        name = "Archer",
                        description = "Ranged unit of the alliance.",
                        factionId = FactionId,
                        attack = 12,
                        defense = 8,
                        health = 25,
                        speed = 6,
                        shots = Ranged ? (int?)12 : null,
                        rangedAttackRange = Ranged ? (int?)8 : null,
                        minimumDamage = 3,
                        maximumDamage = 7,
                        initiative = 1.5,
                        morale = 4,
                        luck = 2
                    });
            }

            path.ShouldBe(request.Method == HttpMethod.Post
                ? "/api/v1/units"
                : "/api/v1/units/" + UnitId);
            Saves++;
            SaveMethod = request.Method;
            SavedUnit = JsonSerializer.Deserialize<JsonElement>(
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
                ? JsonResponse(HttpStatusCode.Created, new { id = UnitId })
                : new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        private HttpResponseMessage GetFactions(Uri uri)
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            query["limit"].ShouldBe("20");
            string? search = query["search"];
            FactionRequests.Add(search);
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
