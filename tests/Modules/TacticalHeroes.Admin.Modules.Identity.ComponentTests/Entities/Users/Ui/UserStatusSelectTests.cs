using System.Net;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Entities.Users.Ui;

public sealed class UserStatusSelectTests : BunitContext
{
    private readonly UserStatusHandler _handler = new();

    public UserStatusSelectTests()
    {
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

    [Fact(DisplayName = "Loads user statuses and selects the first one by default")]
    public void Load_Should_SelectFirstStatus_When_DefaultIsEnabled()
    {
        string? selectedStatus = null;

        var component = Render<UserStatusSelect>(parameters => parameters
            .Add(select => select.Label, "Статус")
            .Add(select => select.UseFirstAsDefault, true)
            .Add(select => select.ValueChanged, value => selectedStatus = value));

        component.WaitForAssertion(() =>
        {
            selectedStatus.ShouldBe("active");
            component.FindComponents<MudSelectItem<string>>()
                .Select(item => item.Instance.Value)
                .ShouldBe(["active", "blocked"]);
        });
    }

    [Fact(DisplayName = "Shows a load error and retries loading user statuses")]
    public void Load_Should_Retry_When_FirstRequestFails()
    {
        _handler.FailNextRequest = true;
        var component = Render<UserStatusSelect>(parameters => parameters
            .Add(select => select.Label, "Статус"));

        component.WaitForAssertion(() =>
            component.Markup.ShouldContain("Повторить"));

        component.FindAll("button")
            .Single(button => button.TextContent.Contains(
                "Повторить",
                StringComparison.Ordinal))
            .Click();

        component.WaitForAssertion(() =>
        {
            _handler.GetCount.ShouldBe(2);
            component.FindComponents<MudSelectItem<string>>().Count.ShouldBe(2);
        });
    }

    private sealed class UserStatusHandler : HttpMessageHandler
    {
        public bool FailNextRequest { get; set; }

        public int GetCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Method.ShouldBe(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.ShouldEndWith("/users/statuses");
            GetCount++;

            if (FailNextRequest)
            {
                FailNextRequest = false;
                return Task.FromResult(new HttpResponseMessage(
                    HttpStatusCode.BadRequest));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    [
                      {
                        "id": 1,
                        "name": "active",
                        "displayName": "Active"
                      },
                      {
                        "id": 2,
                        "name": "blocked",
                        "displayName": "Blocked"
                      }
                    ]
                    """,
                    Encoding.UTF8,
                    "application/json"),
            });
        }
    }
}
