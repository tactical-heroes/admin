using System.Net;
using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

using CreateFactionPageComponent =
    TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Ui.CreateFactionPage;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages.CreateFactionPage.Ui;

public sealed class CreateFactionPageTests : BunitContext
{
    private readonly Guid _createdId = Guid.Parse("f341ae7d-69c0-45c6-9a44-110f00127080");
    private readonly CreateFactionHandler _handler;

    public CreateFactionPageTests()
    {
        _handler = new CreateFactionHandler(_createdId);
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

    [Fact(DisplayName = "Creates a valid faction and opens its update page")]
    public void Submit_Should_NavigateToUpdatePage_When_FactionIsValid()
    {
        var component = Render<CreateFactionPageComponent>();
        component.Find("input").Change("Northern Alliance");
        component.Find("textarea").Change("A defensive coalition.");

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            Services.GetRequiredService<NavigationManager>().Uri
                .ShouldEndWith(CompendiumRoutes.Faction(_createdId));
        });
    }

    [Fact(DisplayName = "Does not create a faction when required fields are empty")]
    public void Submit_Should_DisplayValidationErrors_When_RequiredFieldsAreEmpty()
    {
        var component = Render<CreateFactionPageComponent>();

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите название фракции");
            component.Markup.ShouldContain("Укажите описание фракции");
        });
    }

    [Fact(DisplayName = "Displays a server validation error on its field")]
    public void Submit_Should_DisplayFieldError_When_ServerRejectsFactionName()
    {
        _handler.RejectName = true;
        var component = Render<CreateFactionPageComponent>();
        component.Find("input").Change("Northern Alliance");
        component.Find("textarea").Change("A defensive coalition.");

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() => component.Markup.ShouldContain(
            "A faction with this name already exists."));
    }

    private sealed class CreateFactionHandler(Guid createdId) : HttpMessageHandler
    {
        public int PostCount { get; private set; }

        public bool RejectName { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Method.ShouldBe(HttpMethod.Post);
            PostCount++;

            var response = RejectName
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = JsonContent(
                        """
                        {
                          "status": 400,
                          "errors": {
                            "Name": [
                              "A faction with this name already exists."
                            ]
                          }
                        }
                        """),
                }
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = JsonContent($$"""{"id":"{{createdId:D}}"}"""),
                };

            return Task.FromResult(response);
        }

        private static StringContent JsonContent(string value)
        {
            return new StringContent(value, Encoding.UTF8, "application/json");
        }
    }
}
