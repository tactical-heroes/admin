using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using TacticalHeroes.Admin.Api.Authentication;
using TacticalHeroes.Admin.Api.DependencyInjection;
using TacticalHeroes.Admin.Api.Generated;

namespace TacticalHeroes.Admin.Api.UnitTests.Authentication;

public sealed class SessionRefreshHandlerTests
{
    [Theory(DisplayName = "SendAsync should request refresh only when successful response contains refresh signal")]
    [InlineData(204, "true", true)]
    [InlineData(200, "TRUE", true)]
    [InlineData(204, null, false)]
    [InlineData(204, "false", false)]
    [InlineData(400, "true", false)]
    [InlineData(401, "true", false)]
    [InlineData(500, "true", false)]
    public async Task SendAsync_Should_RequestRefreshOnly_When_SuccessfulResponseContainsRefreshSignal(
        int statusCode, string? signal, bool expected)
    {
        await using var provider = CreateProvider((HttpStatusCode)statusCode, signal);
        await using var scope = provider.CreateAsyncScope();
        var state = scope.ServiceProvider.GetRequiredService<SessionRefreshState>();
        var notifications = 0;
        state.RefreshRequested += () => notifications++;
        var client = scope.ServiceProvider.GetRequiredKeyedService<HttpClient>("TacticalHeroesApi");

        using var response = await client.PostAsync(
            "any/future-operation", content: null, TestContext.Current.CancellationToken);

        ((int)response.StatusCode).ShouldBe(statusCode);
        state.IsRequired.ShouldBe(expected);
        notifications.ShouldBe(expected ? 1 : 0);
    }

    [Fact(DisplayName = "SendAsync should isolate refresh state when clients use different scopes")]
    public async Task SendAsync_Should_IsolateRefreshState_When_ClientsUseDifferentScopes()
    {
        await using var provider = CreateProvider(HttpStatusCode.NoContent, "true");
        await using var first = provider.CreateAsyncScope();
        await using var second = provider.CreateAsyncScope();
        var firstState = first.ServiceProvider.GetRequiredService<SessionRefreshState>();
        var secondState = second.ServiceProvider.GetRequiredService<SessionRefreshState>();
        var client = first.ServiceProvider.GetRequiredService<TacticalHeroesApiClient>();

        await client.Api.V1.Auth.ChangePassword.PostAsync(
            new(), cancellationToken: TestContext.Current.CancellationToken);

        firstState.IsRequired.ShouldBeTrue();
        secondState.IsRequired.ShouldBeFalse();
    }

    [Fact(DisplayName = "SendAsync should notify once when multiple responses require refresh")]
    public async Task SendAsync_Should_NotifyOnce_When_MultipleResponsesRequireRefresh()
    {
        await using var provider = CreateProvider(HttpStatusCode.NoContent, "true");
        await using var scope = provider.CreateAsyncScope();
        var notifications = 0;
        scope.ServiceProvider.GetRequiredService<SessionRefreshState>().RefreshRequested += () => notifications++;
        var client = scope.ServiceProvider.GetRequiredKeyedService<HttpClient>("TacticalHeroesApi");

        using var first = await client.PostAsync("first", content: null, TestContext.Current.CancellationToken);
        using var second = await client.PutAsync("second", content: null, TestContext.Current.CancellationToken);

        notifications.ShouldBe(1);
    }

    private static ServiceProvider CreateProvider(HttpStatusCode statusCode, string? signal)
    {
        var services = new ServiceCollection();
        services.AddTacticalHeroesApiClient(
            static _ => new Uri("https://api.example.test"), static _ => TimeSpan.FromSeconds(30));
        services.Configure<HttpClientFactoryOptions>("TacticalHeroesApi", options =>
            options.HttpMessageHandlerBuilderActions.Add(builder =>
                builder.PrimaryHandler = new ResponseHandler(statusCode, signal)));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private sealed class ResponseHandler(HttpStatusCode statusCode, string? signal) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode);
            if (signal is not null)
            {
                response.Headers.Add(SessionRefreshState.HeaderName, signal);
            }

            return Task.FromResult(response);
        }
    }
}
