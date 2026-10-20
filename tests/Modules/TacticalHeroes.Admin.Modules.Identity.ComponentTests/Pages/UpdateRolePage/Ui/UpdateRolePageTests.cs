using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Ui;
using TacticalHeroes.Admin.Shared.Ui.Forms;
using TacticalHeroes.Admin.Shared.Ui.Layout;

using UpdateRolePageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Ui.UpdateRolePage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.UpdateRolePage.Ui;

public sealed class UpdateRolePageTests : BunitContext
{
    private readonly Guid _roleId = Guid.Parse("4a78777c-0bbe-43be-9976-99bcf2e17bf0");
    private readonly UpdateRoleHandler _handler;

    public UpdateRolePageTests()
    {
        _handler = new UpdateRoleHandler(_roleId);
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
        var component = Render<UpdateRolePageComponent>(parameters => parameters
            .Add(page => page.Id, _roleId));

        component.WaitForAssertion(() =>
        {
            component.FindComponent<PageBackButton>().Find("a").GetAttribute("href")
                .ShouldBe(IdentityRoutes.Roles);
            component.FindComponent<EditFormActions>().Find("a").GetAttribute("href")
                .ShouldBe(IdentityRoutes.Roles);
        });
    }

    [Fact(DisplayName = "Submit should navigate to list when role is valid")]
    public void Submit_Should_NavigateToList_When_RoleIsValid()
    {
        var component = Render<UpdateRolePageComponent>(parameters => parameters
            .Add(page => page.Id, _roleId));
        component.WaitForElement(".permission-table");
        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(1);
            using var body = JsonDocument.Parse(_handler.RequestBody!);
            body.RootElement.GetProperty("isDefault").GetBoolean().ShouldBeTrue();
            body.RootElement.GetProperty("claims").GetArrayLength().ShouldBe(0);
            Services.GetRequiredService<NavigationManager>().Uri
                .ShouldEndWith(IdentityRoutes.Roles);
        });
    }

    [Fact(DisplayName = "Submit should send permission and default flag when role access is edited")]
    public async Task Submit_Should_SendPermissionAndDefaultFlag_When_RoleAccessIsEdited()
    {
        var component = Render<UpdateRolePageComponent>(parameters => parameters.Add(page => page.Id, _roleId));
        await component.InvokeAsync(() => component.FindComponent<MudSwitch<bool>>().Instance.ValueChanged.InvokeAsync(false));
        var claims = component.FindComponent<ClaimsEditor>();
        await component.InvokeAsync(() => claims.FindComponents<MudCheckBox<bool?>>()
            .Single(box => box.Instance.UserAttributes?["aria-label"]?.ToString() == "Пользователи: Просмотр")
            .Instance.ValueChanged.InvokeAsync(true));

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(1);
            using var body = JsonDocument.Parse(_handler.RequestBody!);
            body.RootElement.GetProperty("isDefault").GetBoolean().ShouldBeFalse();
            var sentClaims = body.RootElement.GetProperty("claims");
            sentClaims.GetArrayLength().ShouldBe(1);
            sentClaims[0].GetProperty("type").GetString().ShouldBe("permission");
            sentClaims[0].GetProperty("value").GetString().ShouldBe("identity.users.read");
        });
    }

    [Fact(DisplayName = "Submit should display validation error when role name is empty")]
    public void Submit_Should_DisplayValidationError_When_RoleNameIsEmpty()
    {
        var component = Render<UpdateRolePageComponent>(parameters => parameters
            .Add(page => page.Id, _roleId));
        component.WaitForElement("input").Change(string.Empty);

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите название роли");
        });
    }

    private sealed class UpdateRoleHandler(Guid roleId) : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        public int PutCount { get; private set; }

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
                        $$"""
                        {
                          "id": "{{roleId:D}}",
                          "name": "Administrators",
                          "isDefault": true,
                          "claims": []
                        }
                        """,
                        Encoding.UTF8,
                        "application/json"),
                };
            }

            request.Method.ShouldBe(HttpMethod.Put);
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            PutCount++;

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
