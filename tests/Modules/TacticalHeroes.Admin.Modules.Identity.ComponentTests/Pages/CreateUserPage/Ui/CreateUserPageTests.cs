using System.Net;
using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

using CreateUserPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Ui.CreateUserPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.CreateUserPage.Ui;

public sealed class CreateUserPageTests : BunitContext
{
    private readonly Guid _createdId = Guid.Parse("2f56eb88-8f57-47ef-884f-eae99d5f4ab8");
    private readonly CreateUserHandler _handler;

    public CreateUserPageTests()
    {
        _handler = new CreateUserHandler(_createdId);
        Services.AddMudServices();
        Services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"),
            static _ => TimeSpan.FromSeconds(30));
        Services.Configure<HttpClientFactoryOptions>(
            "TacticalHeroesApi",
            options => options.HttpMessageHandlerBuilderActions.Add(
                builder => builder.PrimaryHandler = _handler));
        Services.AddIdentityAdminModule();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Creates a valid user and opens its update page")]
    public void Submit_Should_NavigateToUpdatePage_When_UserIsValid()
    {
        var component = Render<CreateUserPageComponent>();
        IReadOnlyList<AngleSharp.Dom.IElement> inputs = component.WaitForElements("input");
        inputs[0].Change("admin@example.test");
        inputs[1].Change("admin");
        inputs[2].Change("StrongPassword1!");

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            Services.GetRequiredService<NavigationManager>().Uri
                .ShouldEndWith(IdentityRoutes.User(_createdId));
        });
    }

    [Fact(DisplayName = "Does not create a user when account fields are empty")]
    public void Submit_Should_DisplayValidationErrors_When_AccountFieldsAreEmpty()
    {
        var component = Render<CreateUserPageComponent>();
        component.WaitForElement(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите email");
            component.Markup.ShouldContain("Укажите имя пользователя");
            component.Markup.ShouldContain("Укажите пароль");
        });
    }

    private sealed class CreateUserHandler(Guid createdId) : HttpMessageHandler
    {
        public int PostCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        [
                          {
                            "id": 1,
                            "name": "active",
                            "displayName": "Active"
                          }
                        ]
                        """,
                        Encoding.UTF8,
                        "application/json"),
                });
            }

            request.Method.ShouldBe(HttpMethod.Post);
            PostCount++;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    $$"""{"id":"{{createdId:D}}"}""",
                    Encoding.UTF8,
                    "application/json"),
            });
        }
    }
}
