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

using HeroListPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.HeroListPage.Ui.HeroListPage;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages.HeroListPage.Ui;

public sealed class HeroListPageTests : BunitContext
{
    private static readonly Guid EntityId = Guid.Parse("f341ae7d-69c0-45c6-9a44-110f00127080");
    private readonly ListHandler _handler = new();

    public HeroListPageTests()
    {
        Services.AddMudServices();
        Services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"),
            static _ => TimeSpan.FromSeconds(30));
        Services.Configure<HttpClientFactoryOptions>("TacticalHeroesApi",
            options => options.HttpMessageHandlerBuilderActions.Add(
                builder => builder.PrimaryHandler = _handler));
        Services.AddCompendiumAdminModule();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Render should show rows and actions when load succeeds")]
    public void Render_Should_ShowRowsAndActions_When_LoadSucceeds()
    {
        var component = Render<HeroListPageComponent>();

        component.WaitForAssertion(() =>
        {
            component.Markup.ShouldContain("Catherine");
            component.Markup.ShouldContain("Northern Alliance");
            component.FindAll("a").Select(link => link.GetAttribute("href"))
                .ShouldContain(CompendiumRoutes.CreateHero);
            component.FindAll("a").Select(link => link.GetAttribute("href"))
                .ShouldContain(CompendiumRoutes.Hero(EntityId));
        });
    }

    [Fact(DisplayName = "Render should request selected page when query contains pagination")]
    public void Render_Should_RequestSelectedPage_When_QueryContainsPagination()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            CompendiumRoutes.Heroes + "?page=2&pageSize=25");

        var component = Render<HeroListPageComponent>();

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

        var component = Render<HeroListPageComponent>();

        component.WaitForAssertion(() => component.Markup.ShouldContain("Герои не найдены"));
        component.FindAll("button[aria-label^='Удалить']").ShouldBeEmpty();
    }

    [Fact(DisplayName = "Render should reload rows when load error is retried")]
    public void Render_Should_ReloadRows_When_LoadErrorIsRetried()
    {
        _handler.Fail = true;
        var component = Render<HeroListPageComponent>();
        component.WaitForAssertion(() => component.Markup.ShouldContain("Load failed."));
        _handler.Fail = false;

        component.FindAll("button").Single(button => button.TextContent.Contains("Повторить", StringComparison.Ordinal)).Click();

        component.WaitForAssertion(() =>
        {
            _handler.Requests.Count.ShouldBe(2);
            component.Markup.ShouldContain("Catherine");
            component.Markup.ShouldNotContain("Load failed.");
        });
    }

    [Fact(DisplayName = "DeleteHeroAsync should delete and reload when deletion is confirmed")]
    public async Task DeleteHeroAsync_Should_DeleteAndReload_When_DeletionIsConfirmed()
    {
        var dialogs = Render<MudDialogProvider>();
        var component = Render<HeroListPageComponent>();
        var button = component.WaitForElement("button[aria-label^='Удалить']");

        Task click = button.ClickAsync(new MouseEventArgs());
        dialogs.WaitForAssertion(() => dialogs.Markup.ShouldContain("Catherine"));
        dialogs.FindAll("button").Single(candidate => candidate.TextContent.Trim() == "Удалить").Click();
        await click;

        component.WaitForAssertion(() =>
        {
            _handler.DeletedId.ShouldBe(EntityId);
            _handler.Requests.Count.ShouldBe(2);
            component.Markup.ShouldContain("Герои не найдены");
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
                request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/heroes/" + EntityId);
                DeletedId = EntityId;
                Empty = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }

            request.Method.ShouldBe(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.ShouldBe("/api/v1/heroes");
            Requests.Add(request.RequestUri);
            var query = HttpUtility.ParseQueryString(request.RequestUri.Query);
            int page = int.Parse(query["pageNumber"]!);
            int size = int.Parse(query["pageSize"]!);
            string items = Empty ? "[]" : $$"""[{"id":"{{EntityId}}","name":"Catherine","factionId":"d1703c92-a294-4584-8a9a-78469111363d","factionName":"Northern Alliance"}]""";
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
