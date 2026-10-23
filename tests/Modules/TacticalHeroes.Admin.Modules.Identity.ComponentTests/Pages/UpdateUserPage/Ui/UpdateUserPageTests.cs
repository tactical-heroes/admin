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
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Ui;
using TacticalHeroes.Admin.Shared.Ui.Forms;
using TacticalHeroes.Admin.Shared.Ui.Inputs;
using TacticalHeroes.Admin.Shared.Ui.Layout;

using UpdateUserPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Ui.UpdateUserPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.UpdateUserPage.Ui;

public sealed class UpdateUserPageTests : BunitContext
{
    private readonly Guid _userId = Guid.Parse("2f56eb88-8f57-47ef-884f-eae99d5f4ab8");
    private readonly UpdateUserHandler _handler;
    private readonly Bunit.TestDoubles.BunitAuthorizationContext _authorization;

    public UpdateUserPageTests()
    {
        _handler = new UpdateUserHandler(_userId);
        Services.AddMudServices();
        _authorization = AddAuthorization();
        _authorization.SetAuthorized("Administrator");
        _authorization.SetClaims([.. Permissions.All.Select(permission =>
            new System.Security.Claims.Claim("permission", permission))]);
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

    [Fact(DisplayName = "Render should load user without saving when only read permission is granted")]
    public void Render_Should_LoadUserWithoutSaving_When_OnlyReadPermissionIsGranted()
    {
        _authorization.SetClaims(new System.Security.Claims.Claim("permission", Permissions.Users.Read));

        var component = Render<UpdateUserPageComponent>(parameters => parameters.Add(page => page.Id, _userId));

        component.WaitForElement("input").GetAttribute("value").ShouldBe("admin@example.test");
        component.FindAll(".submit-action").ShouldBeEmpty();
        component.FindComponents<ClaimsEditor>().ShouldBeEmpty();
        component.FindComponents<RoleMultiSelect>().ShouldBeEmpty();
        _handler.PutCount.ShouldBe(0);
    }

    [Fact(DisplayName = "Render should link back and cancel to list when form is displayed")]
    public void Render_Should_LinkBackAndCancelToList_When_FormIsDisplayed()
    {
        var component = Render<UpdateUserPageComponent>(parameters => parameters
            .Add(page => page.Id, _userId));

        component.WaitForAssertion(() =>
        {
            component.FindComponent<PageBackButton>().Find("a").GetAttribute("href")
                .ShouldBe(IdentityRoutes.Users);
            component.FindComponent<EditFormActions>().Find("a").GetAttribute("href")
                .ShouldBe(IdentityRoutes.Users);
        });
    }

    [Fact(DisplayName = "Render should display selected status when user details are loaded")]
    public void Render_Should_DisplaySelectedStatus_When_UserDetailsAreLoaded()
    {
        var component = Render<UpdateUserPageComponent>(parameters => parameters.Add(page => page.Id, _userId));

        component.WaitForAssertion(() => component.FindComponent<EnumerationSelect>()
            .Find("input").GetAttribute("value").ShouldBe("Active"));
    }

    [Fact(DisplayName = "Submit should navigate to list when user is valid")]
    public void Submit_Should_NavigateToList_When_UserIsValid()
    {
        var component = Render<UpdateUserPageComponent>(parameters => parameters
            .Add(page => page.Id, _userId));
        component.WaitForElement(".permission-table");
        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(1);
            using var body = JsonDocument.Parse(_handler.RequestBody!);
            body.RootElement.GetProperty("roleIds").EnumerateArray().Select(value => value.GetGuid())
                .ShouldBe([IdentityCatalogResponses.RoleId]);
            body.RootElement.GetProperty("claims").GetArrayLength().ShouldBe(0);
            Services.GetRequiredService<NavigationManager>().Uri
                .ShouldEndWith(IdentityRoutes.Users);
        });
    }

    [Theory(DisplayName = "Submit should send selected roles and catalog claims when access is edited")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Submit_Should_SendSelectedRolesAndCatalogClaims_When_AccessIsEdited(bool keepRole)
    {
        var component = Render<UpdateUserPageComponent>(parameters => parameters.Add(page => page.Id, _userId));
        Guid[] roles = keepRole ? [IdentityCatalogResponses.RoleId] : [];
        if (!keepRole)
        {
            await component.InvokeAsync(() => component.FindComponent<RoleMultiSelect>()
                .Instance.ValuesChanged.InvokeAsync([.. roles]));
        }
        var claims = component.FindComponent<ClaimsEditor>();
        await component.InvokeAsync(() => claims.FindComponents<MudCheckBox<bool?>>()
            .Single(box => box.Instance.UserAttributes?["aria-label"]?.ToString() == "Пользователи: Просмотр")
            .Instance.ValueChanged.InvokeAsync(true));

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(1);
            using var body = JsonDocument.Parse(_handler.RequestBody!);
            body.RootElement.GetProperty("roleIds").EnumerateArray().Select(value => value.GetGuid()).ShouldBe(roles);
            var sentClaims = body.RootElement.GetProperty("claims");
            sentClaims.GetArrayLength().ShouldBe(1);
            sentClaims[0].GetProperty("type").GetString().ShouldBe("permission");
            sentClaims[0].GetProperty("value").GetString().ShouldBe("identity.users.read");
        });
    }

    [Fact(DisplayName = "Submit should reload refreshed session when current user is saved")]
    public void NavigateAfterSaveAsync_Should_RefreshSession_When_CurrentUserIsSaved()
    {
        _authorization.SetClaims(
            [.. Permissions.All.Select(permission => new System.Security.Claims.Claim("permission", permission)),
             new System.Security.Claims.Claim("sub", _userId.ToString())]);
        var component = Render<UpdateUserPageComponent>(parameters => parameters.Add(page => page.Id, _userId));
        component.WaitForElement(".permission-table");

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(1);
            Services.GetRequiredService<NavigationManager>().Uri.ShouldEndWith(IdentityRoutes.AuthenticationRefresh);
            Services.GetRequiredService<NavigationManager>()
                .ShouldBeOfType<Bunit.TestDoubles.BunitNavigationManager>()
                .History.First().Options.ForceLoad.ShouldBeTrue();
        });
    }

    [Fact(DisplayName = "Submit should display validation error when email is empty")]
    public void Submit_Should_DisplayValidationError_When_EmailIsEmpty()
    {
        var component = Render<UpdateUserPageComponent>(parameters => parameters
            .Add(page => page.Id, _userId));
        component.WaitForElement("input").Change(string.Empty);

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите email");
        });
    }

    private sealed class UpdateUserHandler(Guid userId) : HttpMessageHandler
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

            if (request.Method == HttpMethod.Get &&
                request.RequestUri!.AbsolutePath.EndsWith("/statuses", StringComparison.Ordinal))
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

            if (request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        $$"""
                        {
                          "id": "{{userId:D}}",
                          "email": "admin@example.test",
                          "userName": "admin",
                          "isConfirmed": true,
                          "status": "active",
                          "statusDisplayName": "Active",
                          "claims": [],
                          "roleIds": ["{{IdentityCatalogResponses.RoleId:D}}"]
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
