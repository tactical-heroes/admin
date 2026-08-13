using System.Net;
using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

using CreateRolePageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Ui.CreateRolePage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.CreateRolePage.Ui;

public sealed class CreateRolePageTests : BunitContext
{
    private readonly Guid _createdId = Guid.Parse("4a78777c-0bbe-43be-9976-99bcf2e17bf0");
    private readonly CreateRoleHandler _handler;

    public CreateRolePageTests()
    {
        _handler = new CreateRoleHandler(_createdId);
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

    [Fact(DisplayName = "Creates a valid role and opens its update page")]
    public void Submit_Should_NavigateToUpdatePage_When_RoleIsValid()
    {
        var component = Render<CreateRolePageComponent>();
        component.Find("input").Change("Administrators");

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            Services.GetRequiredService<NavigationManager>().Uri
                .ShouldEndWith(IdentityRoutes.Role(_createdId));
        });
    }

    [Fact(DisplayName = "Creates a valid role with a claim")]
    public void Submit_Should_CreateRole_When_ClaimIsValid()
    {
        var component = Render<CreateRolePageComponent>();
        component.Find("input").Change("Administrators");
        component.FindAll("button")
            .Single(button => button.TextContent.Contains("Добавить", StringComparison.Ordinal))
            .Click();

        var inputs = component.FindAll("input");
        inputs[1].Change("permission");
        inputs[2].Change("read");

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() => _handler.PostCount.ShouldBe(1));
    }

    [Fact(DisplayName = "Does not create a role when its name is empty")]
    public void Submit_Should_DisplayValidationError_When_RoleNameIsEmpty()
    {
        var component = Render<CreateRolePageComponent>();

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите название роли");
        });
    }

    private sealed class CreateRoleHandler(Guid createdId) : HttpMessageHandler
    {
        public int PostCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
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
