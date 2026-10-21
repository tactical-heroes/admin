using System.Net;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.HeroListPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Api;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Api;

public sealed class RequiredCollectionMappingTests : IDisposable
{
    private readonly ServiceProvider _services;

    public RequiredCollectionMappingTests()
    {
        var services = new ServiceCollection();
        services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"),
            static _ => TimeSpan.FromSeconds(30));
        services.Configure<HttpClientFactoryOptions>("TacticalHeroesApi",
            options => options.HttpMessageHandlerBuilderActions.Add(builder => builder.PrimaryHandler = new ResponseHandler()));
        services.AddCompendiumAdminModule();
        _services = services.BuildServiceProvider();
    }

    [Theory(DisplayName = "GetPageAsync should reject response when required collection is null")]
    [InlineData("factions")]
    [InlineData("heroes")]
    [InlineData("units")]
    public async Task GetPageAsync_Should_RejectResponse_When_RequiredCollectionIsNull(string resource)
    {
        var token = Xunit.TestContext.Current.CancellationToken;
        Func<Task> load = resource switch
        {
            "factions" => () => _services.GetRequiredService<FactionListApi>().GetPageAsync(1, 20, [], token),
            "heroes" => () => _services.GetRequiredService<HeroListApi>().GetPageAsync(1, 20, [], token),
            "units" => () => _services.GetRequiredService<UnitListApi>().GetPageAsync(1, 20, [], token),
            _ => throw new ArgumentOutOfRangeException(nameof(resource)),
        };

        await Should.ThrowAsync<ArgumentNullException>(load);
    }

    public void Dispose() => _services.Dispose();

    private sealed class ResponseHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"items":null,"pageNumber":1,"pageSize":20,"totalCount":0,"totalPages":0}""",
                    Encoding.UTF8,
                    "application/json"),
            });
        }
    }
}
