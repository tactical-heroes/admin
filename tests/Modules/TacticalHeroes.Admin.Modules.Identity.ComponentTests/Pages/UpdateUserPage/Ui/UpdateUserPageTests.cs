using System.Net;
using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

using UpdateUserPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Ui.UpdateUserPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.UpdateUserPage.Ui;

public sealed class UpdateUserPageTests : BunitContext
{
    private readonly Guid _userId = Guid.Parse("2f56eb88-8f57-47ef-884f-eae99d5f4ab8");
    private readonly UpdateUserHandler _handler;

    public UpdateUserPageTests()
    {
        _handler = new UpdateUserHandler(_userId);
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

    [Fact(DisplayName = "Updates a valid user and returns to the list")]
    public void Submit_Should_NavigateToList_When_UserIsValid()
    {
        var component = Render<UpdateUserPageComponent>(parameters => parameters
            .Add(page => page.Id, _userId));
        component.WaitForElement(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PutCount.ShouldBe(1);
            Services.GetRequiredService<NavigationManager>().Uri
                .ShouldEndWith(IdentityRoutes.Users);
        });
    }

    private sealed class UpdateUserHandler(Guid userId) : HttpMessageHandler
    {
        public int PutCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get &&
                request.RequestUri!.AbsolutePath.EndsWith("/statuses", StringComparison.Ordinal))
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

            if (request.Method == HttpMethod.Get)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
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
