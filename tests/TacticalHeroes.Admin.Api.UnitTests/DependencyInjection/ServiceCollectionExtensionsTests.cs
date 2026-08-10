using System.Net;
using System.Net.Http.Headers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using TacticalHeroes.Admin.Api.DependencyInjection;

namespace TacticalHeroes.Admin.Api.UnitTests.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "Retries safe requests after transient failures")]
    public async Task AddTacticalHeroesApiClient_Should_RetryGet_When_TransientFailureOccurs()
    {
        var handler = new SequenceHandler(
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.OK);
        using var serviceProvider = CreateServiceProvider(handler);
        var client = serviceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient("TacticalHeroesApi");

        using var response = await client.GetAsync("factions", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        handler.AttemptCount.ShouldBe(2);
    }

    [Fact(DisplayName = "Does not retry unsafe requests after transient failures")]
    public async Task AddTacticalHeroesApiClient_Should_NotRetryPost_When_TransientFailureOccurs()
    {
        var handler = new SequenceHandler(HttpStatusCode.ServiceUnavailable);
        using var serviceProvider = CreateServiceProvider(handler);
        var client = serviceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient("TacticalHeroesApi");

        using var response = await client.PostAsync(
            "factions",
            content: null,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        handler.AttemptCount.ShouldBe(1);
    }

    private static ServiceProvider CreateServiceProvider(HttpMessageHandler handler)
    {
        var services = new ServiceCollection();

        services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"),
            static _ => TimeSpan.FromSeconds(30));
        services.Configure<HttpClientFactoryOptions>(
            "TacticalHeroesApi",
            options => options.HttpMessageHandlerBuilderActions.Add(
                builder => builder.PrimaryHandler = handler));

        return services.BuildServiceProvider();
    }

    private sealed class SequenceHandler(params HttpStatusCode[] statusCodes)
        : HttpMessageHandler
    {
        private int _attemptCount;

        public int AttemptCount => _attemptCount;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var attempt = Interlocked.Increment(ref _attemptCount);
            var statusCode = statusCodes[Math.Min(attempt - 1, statusCodes.Length - 1)];
            var response = new HttpResponseMessage(statusCode)
            {
                RequestMessage = request,
            };
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);

            return Task.FromResult(response);
        }
    }
}
