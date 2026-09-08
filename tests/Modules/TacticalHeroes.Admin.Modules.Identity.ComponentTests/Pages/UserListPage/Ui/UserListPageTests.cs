using System.Net;
using System.Text;
using System.Web;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

using UserListPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Ui.UserListPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.UserListPage.Ui;

public sealed class UserListPageTests : BunitContext
{
    private readonly UserListHandler _handler = new();

    public UserListPageTests()
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

    [Theory(DisplayName = "User list restores the email filter from query regardless of name casing")]
    [InlineData("email")]
    [InlineData("Email")]
    [InlineData("EMAIL")]
    public void UserList_Should_ApplyEmailFilter_When_QueryContainsEmail(string queryName)
    {
        const string email = "admin+support@example.test";
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            $"{IdentityRoutes.Users}?{queryName}={Uri.EscapeDataString(email)}");

        var component = Render<UserListPageComponent>();

        component.WaitForAssertion(() =>
        {
            _handler.Email.ShouldBe(email);
            component.Find("input[placeholder='user@example.com']")
                .GetAttribute("value").ShouldBe(email);
            component.Instance.Page.ShouldNotBeNull();
        });
    }

    private sealed class UserListHandler : HttpMessageHandler
    {
        public string? Email { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Method.ShouldBe(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/users");
            Email = HttpUtility.ParseQueryString(request.RequestUri.Query)["email"];

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"items":[],"pageNumber":1,"pageSize":10,"totalCount":0,"totalPages":0}""",
                    Encoding.UTF8,
                    "application/json"),
            });
        }
    }
}
