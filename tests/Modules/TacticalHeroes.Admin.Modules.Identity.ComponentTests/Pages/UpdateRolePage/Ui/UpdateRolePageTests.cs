using System.Net;
using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

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

    [Fact(DisplayName = "Updates a valid role and returns to the list")]
    public void Submit_Should_NavigateToList_When_RoleIsValid()
    {
        var component = Render<UpdateRolePageComponent>(parameters => parameters
            .Add(page => page.Id, _roleId));
        component.WaitForElement(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(1);
            Services.GetRequiredService<NavigationManager>().Uri
                .ShouldEndWith(IdentityRoutes.Roles);
        });
    }

    [Fact(DisplayName = "Does not update a role when its name is empty")]
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
                          "id": "{{roleId:D}}",
                          "name": "Administrators",
                          "claims": []
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
