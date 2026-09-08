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

using RoleListPageComponent = TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Ui.RoleListPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.RoleListPage.Ui;

public sealed class RoleListPageTests : BunitContext
{
    private static readonly Guid EntityId = Guid.Parse("f341ae7d-69c0-45c6-9a44-110f00127080");
    private readonly ListHandler _handler = new();

    public RoleListPageTests()
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

    [Fact(DisplayName = "The role list renders API data and create and edit links")]
    public void Render_Should_ShowRowsAndActions_When_LoadSucceeds()
    {
        var component = Render<RoleListPageComponent>();

        component.WaitForAssertion(() =>
        {
            component.Markup.ShouldContain("Administrator");
            component.FindAll("a").Select(link => link.GetAttribute("href"))
                .ShouldContain(IdentityRoutes.CreateRole);
            component.FindAll("a").Select(link => link.GetAttribute("href"))
                .ShouldContain(IdentityRoutes.Role(EntityId));
        });
    }

    [Fact(DisplayName = "The role list restores pagination from the page URL")]
    public void Render_Should_RequestSelectedPage_When_QueryContainsPagination()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            IdentityRoutes.Roles + "?page=2&pageSize=25");

        var component = Render<RoleListPageComponent>();

        component.WaitForAssertion(() =>
        {
            _handler.Requests.Count.ShouldBe(1);
            var query = HttpUtility.ParseQueryString(_handler.Requests[0].Query);
            query["pageNumber"].ShouldBe("2");
            query["pageSize"].ShouldBe("25");
        });
    }

    [Fact(DisplayName = "The role list shows an empty state when the API returns no rows")]
    public void Render_Should_ShowEmptyState_When_PageIsEmpty()
    {
        _handler.Empty = true;

        var component = Render<RoleListPageComponent>();

        component.WaitForAssertion(() => component.Markup.ShouldContain("Роли не найдены"));
        component.FindAll("button[aria-label^='Удалить']").ShouldBeEmpty();
    }

    [Fact(DisplayName = "The role list can retry after a failed request")]
    public void Render_Should_ReloadRows_When_LoadErrorIsRetried()
    {
        _handler.Fail = true;
        var component = Render<RoleListPageComponent>();
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

    [Fact(DisplayName = "Deleting a role uses its identifier and refreshes the list")]
    public async Task DeleteRoleAsync_Should_DeleteAndReload_When_DeletionIsConfirmed()
    {
        var dialogs = Render<MudDialogProvider>();
        var component = Render<RoleListPageComponent>();
        var button = component.WaitForElement("button[aria-label^='Удалить']");

        Task click = button.ClickAsync(new MouseEventArgs());
        dialogs.WaitForAssertion(() => dialogs.Markup.ShouldContain("Administrator"));
        dialogs.FindAll("button").Single(candidate => candidate.TextContent.Trim() == "Удалить").Click();
        await click;

        component.WaitForAssertion(() =>
        {
            _handler.DeletedId.ShouldBe(EntityId);
            _handler.Requests.Count.ShouldBe(2);
            component.Markup.ShouldContain("Роли не найдены");
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
                request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/roles/" + EntityId);
                DeletedId = EntityId;
                Empty = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }

            request.Method.ShouldBe(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/roles");
            Requests.Add(request.RequestUri);
            var query = HttpUtility.ParseQueryString(request.RequestUri.Query);
            int page = int.Parse(query["pageNumber"]!);
            int size = int.Parse(query["pageSize"]!);
            string items = Empty ? "[]" : $$"""[{"id":"{{EntityId}}","name":"Administrator"}]""";
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
