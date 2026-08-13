using System.Net;
using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

using UpdateFactionPageComponent =
    TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Ui.UpdateFactionPage;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages.UpdateFactionPage.Ui;

public sealed class UpdateFactionPageTests : BunitContext
{
    private readonly Guid _factionId = Guid.Parse("f341ae7d-69c0-45c6-9a44-110f00127080");
    private readonly UpdateFactionHandler _handler;

    public UpdateFactionPageTests()
    {
        _handler = new UpdateFactionHandler(_factionId);
        Services.AddMudServices();
        Services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"),
            static _ => TimeSpan.FromSeconds(30));
        Services.Configure<HttpClientFactoryOptions>(
            "TacticalHeroesApi",
            options => options.HttpMessageHandlerBuilderActions.Add(
                builder => builder.PrimaryHandler = _handler));
        Services.AddCompendiumAdminModule();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Updates a valid faction and returns to the list")]
    public void Submit_Should_NavigateToList_When_FactionIsValid()
    {
        var component = Render<UpdateFactionPageComponent>(parameters => parameters
            .Add(page => page.Id, _factionId));
        component.WaitForElement(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(1);
            Services.GetRequiredService<NavigationManager>().Uri
                .ShouldEndWith(CompendiumRoutes.Factions);
        });
    }

    [Fact(DisplayName = "Does not update a faction when its name is empty")]
    public void Submit_Should_DisplayValidationError_When_FactionNameIsEmpty()
    {
        var component = Render<UpdateFactionPageComponent>(parameters => parameters
            .Add(page => page.Id, _factionId));
        component.WaitForElement("input").Change(string.Empty);

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите название фракции");
        });
    }

    private sealed class UpdateFactionHandler(Guid factionId) : HttpMessageHandler
    {
        public int PutCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        $$"""
                        {
                          "id": "{{factionId:D}}",
                          "name": "Northern Alliance",
                          "description": "A defensive coalition."
                        }
                        """,
                        Encoding.UTF8,
                        "application/json"),
                });
            }

            request.Method.ShouldBe(HttpMethod.Put);
            PutCount++;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }
    }
}
