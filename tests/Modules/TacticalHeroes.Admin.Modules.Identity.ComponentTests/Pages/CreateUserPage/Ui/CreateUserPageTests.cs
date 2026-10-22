using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Ui;
using TacticalHeroes.Admin.Shared.Ui.Forms;
using TacticalHeroes.Admin.Shared.Ui.Layout;

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

    [Fact(DisplayName = "Render should link back and cancel to list when form is displayed")]
    public void Render_Should_LinkBackAndCancelToList_When_FormIsDisplayed()
    {
        var component = Render<CreateUserPageComponent>();

        component.WaitForAssertion(() =>
        {
            component.FindComponent<PageBackButton>().Find("a").GetAttribute("href")
                .ShouldBe(IdentityRoutes.Users);
            component.FindComponent<EditFormActions>().Find("a").GetAttribute("href")
                .ShouldBe(IdentityRoutes.Users);
        });
    }

    [Theory(DisplayName = "Submit should send selected roles and navigate when user is valid")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Submit_Should_SendSelectedRolesAndNavigate_When_UserIsValid(bool assignRole)
    {
        var component = Render<CreateUserPageComponent>();
        IReadOnlyList<AngleSharp.Dom.IElement> inputs = component.WaitForElements("input");
        inputs[0].Change("admin@example.test");
        inputs[1].Change("admin");
        inputs[2].Change("StrongPassword1!");
        Guid[] roles = assignRole ? [IdentityCatalogResponses.RoleId] : [];
        if (assignRole)
        {
            await component.InvokeAsync(() => component.FindComponent<RoleSelect>()
                .Instance.ValuesChanged.InvokeAsync([.. roles]));
        }

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            using var body = JsonDocument.Parse(_handler.RequestBody!);
            body.RootElement.GetProperty("roleIds").EnumerateArray().Select(value => value.GetGuid()).ShouldBe(roles);
            Services.GetRequiredService<NavigationManager>().Uri
                .ShouldEndWith(IdentityRoutes.User(_createdId));
        });
    }

    [Fact(DisplayName = "Submit should display validation errors when account fields are empty")]
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

            if (request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
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
                };
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
