using System.Net;
using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Shared.Ui.Forms;
using TacticalHeroes.Admin.Shared.Ui.Layout;

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

    [Fact(DisplayName = "Render should link back and cancel to list when form is displayed")]
    public void Render_Should_LinkBackAndCancelToList_When_FormIsDisplayed()
    {
        var component = Render<UpdateFactionPageComponent>(parameters => parameters
            .Add(page => page.Id, _factionId));

        component.WaitForAssertion(() =>
        {
            component.FindComponent<PageBackButton>().Find("a").GetAttribute("href")
                .ShouldBe(CompendiumRoutes.Factions);
            component.FindComponent<EditFormActions>().Find("a").GetAttribute("href")
                .ShouldBe(CompendiumRoutes.Factions);
        });
    }

    [Fact(DisplayName = "Submit should navigate to list when faction is valid")]
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

    [Fact(DisplayName = "Submit should display validation error when faction name is empty")]
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

    [Fact(DisplayName = "Submit should display field error when server rejects faction name")]
    public void Submit_Should_DisplayFieldError_When_ServerRejectsFactionName()
    {
        _handler.RejectName = true;
        var component = Render<UpdateFactionPageComponent>(parameters => parameters
            .Add(page => page.Id, _factionId));

        component.WaitForElement(".submit-action").Click();

        component.WaitForAssertion(() => component.Markup.ShouldContain(
            "A faction with this name already exists."));
    }

    private sealed class UpdateFactionHandler(Guid factionId) : HttpMessageHandler
    {
        public int PutCount { get; private set; }

        public bool RejectName { get; set; }

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

            var response = RejectName
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(
                        """
                        {
                          "status": 400,
                          "errors": {
                            "Name": [
                              "A faction with this name already exists."
                            ]
                          }
                        }
                        """,
                        Encoding.UTF8,
                        "application/json"),
                }
                : new HttpResponseMessage(HttpStatusCode.NoContent);

            return Task.FromResult(response);
        }
    }
}
