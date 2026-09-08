using System.Net;
using System.Text;
using System.Web;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Api.DependencyInjection;

using UserListPageComponent = TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Ui.UserListPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.UserListPage.Ui;

public sealed class UserListPageTests : BunitContext
{
    private static readonly Guid EntityId = Guid.Parse("f341ae7d-69c0-45c6-9a44-110f00127080");
    private readonly ListHandler _handler = new();

    public UserListPageTests()
    {
        Services.AddMudServices();
        Services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"),
            static _ => TimeSpan.FromSeconds(30));
        Services.Configure<HttpClientFactoryOptions>("TacticalHeroesApi",
            options => options.HttpMessageHandlerBuilderActions.Add(
                builder => builder.PrimaryHandler = _handler));
        Services.AddIdentityAdminModule();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Render should show rows and actions when load succeeds")]
    public void Render_Should_ShowRowsAndActions_When_LoadSucceeds()
    {
        var component = Render<UserListPageComponent>();

        component.WaitForAssertion(() =>
        {
            component.Markup.ShouldContain("Administrator");
            component.FindAll("a").Select(link => link.GetAttribute("href"))
                .ShouldContain(IdentityRoutes.CreateUser);
            component.FindAll("a").Select(link => link.GetAttribute("href"))
                .ShouldContain(IdentityRoutes.User(EntityId));
        });
    }

    [Fact(DisplayName = "Render should request selected page when query contains pagination")]
    public void Render_Should_RequestSelectedPage_When_QueryContainsPagination()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            IdentityRoutes.Users + "?page=2&pageSize=25");

        var component = Render<UserListPageComponent>();

        component.WaitForAssertion(() =>
        {
            _handler.Requests.Count.ShouldBe(1);
            var query = HttpUtility.ParseQueryString(_handler.Requests[0].Query);
            query["pageNumber"].ShouldBe("2");
            query["pageSize"].ShouldBe("25");
        });
    }

    [Fact(DisplayName = "Render should show empty state when page is empty")]
    public void Render_Should_ShowEmptyState_When_PageIsEmpty()
    {
        _handler.Empty = true;

        var component = Render<UserListPageComponent>();

        component.WaitForAssertion(() => component.Markup.ShouldContain("Пользователи не найдены"));
        component.FindAll("button[aria-label^='Удалить']").ShouldBeEmpty();
    }

    [Fact(DisplayName = "Render should reload rows when load error is retried")]
    public void Render_Should_ReloadRows_When_LoadErrorIsRetried()
    {
        _handler.Fail = true;
        var component = Render<UserListPageComponent>();
        component.WaitForAssertion(() => component.Markup.ShouldContain("Load failed."));
        _handler.Fail = false;

        component.FindAll("button").Single(button => button.TextContent.Contains("Повторить", StringComparison.Ordinal)).Click();

        component.WaitForAssertion(() =>
        {
            _handler.Requests.Count.ShouldBe(2);
            component.Markup.ShouldContain("Administrator");
            component.Markup.ShouldNotContain("Load failed.");
        });
    }

    [Fact(DisplayName = "DeleteUserAsync should delete and reload when deletion is confirmed")]
    public async Task DeleteUserAsync_Should_DeleteAndReload_When_DeletionIsConfirmed()
    {
        var dialogs = Render<MudDialogProvider>();
        var component = Render<UserListPageComponent>();
        var button = component.WaitForElement("button[aria-label^='Удалить']");

        Task click = button.ClickAsync(new MouseEventArgs());
        dialogs.WaitForAssertion(() => dialogs.Markup.ShouldContain("Administrator"));
        dialogs.FindAll("button").Single(candidate => candidate.TextContent.Trim() == "Удалить").Click();
        await click;

        component.WaitForAssertion(() =>
        {
            _handler.DeletedId.ShouldBe(EntityId);
            _handler.Requests.Count.ShouldBe(2);
            component.Markup.ShouldContain("Пользователи не найдены");
        });
    }

    [Theory(DisplayName = "Render should restore email filter when query contains email")]
    [InlineData("email")]
    [InlineData("Email")]
    [InlineData("EMAIL")]
    public void Render_Should_RestoreEmailFilter_When_QueryContainsEmail(string queryName)
    {
        const string email = "admin+support@example.test";
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            IdentityRoutes.Users + "?" + queryName + "=" + Uri.EscapeDataString(email));

        var component = Render<UserListPageComponent>();

        component.WaitForAssertion(() =>
        {
            HttpUtility.ParseQueryString(_handler.Requests[0].Query)["email"].ShouldBe(email);
            component.Find("input[placeholder='user@example.com']").GetAttribute("value").ShouldBe(email);
        });
    }

    private sealed class ListHandler : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];
        public bool Empty { get; set; }
        public bool Fail { get; set; }
        public Guid? DeletedId { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Delete)
            {
                request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/users/" + EntityId);
                DeletedId = EntityId;
                Empty = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }

            request.Method.ShouldBe(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/users");
            Requests.Add(request.RequestUri);
            var query = HttpUtility.ParseQueryString(request.RequestUri.Query);
            int page = int.Parse(query["pageNumber"]!);
            int size = int.Parse(query["pageSize"]!);
            string items = Empty ? "[]" : $$"""[{"id":"{{EntityId}}","userName":"Administrator","email":"admin@example.test","status":"active","statusDisplayName":"Active","isConfirmed":true}]""";
            string json = Fail
                ? """{"status":400,"detail":"Load failed."}"""
                : $$"""{"items":{{items}},"pageNumber":{{page}},"pageSize":{{size}},"totalCount":{{(Empty ? 0 : 1)}},"totalPages":{{(Empty ? 0 : 1)}}}""";

            return Task.FromResult(new HttpResponseMessage(Fail ? HttpStatusCode.BadRequest : HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
        }
    }
}
