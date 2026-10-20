using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Shared.Ui.Forms;
using TacticalHeroes.Admin.Shared.Ui.Layout;

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

    [Fact(DisplayName = "Render should link back and cancel to list when form is displayed")]
    public void Render_Should_LinkBackAndCancelToList_When_FormIsDisplayed()
    {
        var component = Render<CreateRolePageComponent>();

        component.WaitForAssertion(() =>
        {
            component.FindComponent<PageBackButton>().Find("a").GetAttribute("href")
                .ShouldBe(IdentityRoutes.Roles);
            component.FindComponent<EditFormActions>().Find("a").GetAttribute("href")
                .ShouldBe(IdentityRoutes.Roles);
        });
    }

    [Fact(DisplayName = "Submit should navigate to update page when role is valid")]
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

    [Fact(DisplayName = "Submit should create role when claim is valid")]
    public async Task Submit_Should_CreateRole_When_ClaimIsValid()
    {
        var component = Render<CreateRolePageComponent>();
        component.Find("input").Change("Administrators");
        component.FindAll("button")
            .Single(button => button.TextContent.Contains("Добавить атрибут", StringComparison.Ordinal))
            .Click();

        var inputs = component.FindAll(".claim-row input");
        inputs[0].Change("custom");
        inputs[1].Change("read");
        await component.InvokeAsync(() => component.FindComponent<MudSwitch<bool>>().Instance.ValueChanged.InvokeAsync(true));

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            using var body = JsonDocument.Parse(_handler.RequestBody!);
            body.RootElement.GetProperty("isDefault").GetBoolean().ShouldBeTrue();
            body.RootElement.GetProperty("claims")[0].GetProperty("type").GetString().ShouldBe("custom");
            body.RootElement.GetProperty("claims")[0].GetProperty("value").GetString().ShouldBe("read");
        });
    }

    [Fact(DisplayName = "Submit should display validation error when role name is empty")]
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

    [Fact(DisplayName = "Submit should display validation errors when claim is empty")]
    public void Submit_Should_DisplayValidationErrors_When_ClaimIsEmpty()
    {
        var component = Render<CreateRolePageComponent>();
        component.Find("input").Change("Administrators");
        component.FindAll("button")
            .Single(button => button.TextContent.Contains("Добавить атрибут", StringComparison.Ordinal))
            .Click();

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите тип атрибута");
            component.Markup.ShouldContain("Укажите значение атрибута");
        });
    }

    private sealed class CreateRoleHandler(Guid createdId) : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        public int PostCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (IdentityCatalogResponses.Create(request) is { } response)
            {
                return response;
            }

            request.Method.ShouldBe(HttpMethod.Post);
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            PostCount++;

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    $$"""{"id":"{{createdId:D}}"}""",
                    Encoding.UTF8,
                    "application/json"),
            };
        }
    }
}
