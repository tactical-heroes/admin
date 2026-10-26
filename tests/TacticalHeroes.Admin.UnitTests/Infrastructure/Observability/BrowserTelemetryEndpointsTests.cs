using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TacticalHeroes.Admin.Infrastructure.Observability;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Observability;

public sealed class BrowserTelemetryEndpointsTests
{
    [Theory(DisplayName = "Browser telemetry should enforce configured bytes when body limit is configured")]
    [InlineData(1048576, 1048576, false, HttpStatusCode.OK)]
    [InlineData(2097152, 1500000, false, HttpStatusCode.OK)]
    [InlineData(2097152, 2097152, true, HttpStatusCode.OK)]
    [InlineData(2097152, 2097153, false, HttpStatusCode.RequestEntityTooLarge)]
    [InlineData(2097152, 2097153, true, HttpStatusCode.RequestEntityTooLarge)]
    public async Task BrowserTelemetry_Should_EnforceConfiguredBytes_When_BodyLimitIsConfigured(
        int limit, int size, bool chunked, HttpStatusCode expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var fixture = await Fixture.StartAsync(cancellationToken, limit);
        using var request = CreateRequest("logs", authenticated: true);
        request.Content = chunked ? new ChunkedContent(size) : new ByteArrayContent(new byte[size]);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        using var response = await fixture.Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expected);
        if (expected == HttpStatusCode.OK)
        {
            fixture.Received.ShouldHaveSingleItem().Body.Length.ShouldBe(size);
        }
        else
        {
            fixture.Received.ShouldBeEmpty();
        }
    }

    [Theory(DisplayName = "Browser telemetry should reject startup when body limit is not positive")]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task BrowserTelemetry_Should_RejectStartup_When_BodyLimitIsNotPositive(int limit)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BrowserTelemetry:MaxRequestBodySize"] = limit.ToString(System.Globalization.CultureInfo.InvariantCulture)
        });
        builder.Services.AddBrowserTelemetry(builder.Configuration);
        await using var app = builder.Build();

        await Should.ThrowAsync<OptionsValidationException>(() => app.StartAsync(TestContext.Current.CancellationToken));
    }

    [Theory(DisplayName = "Browser telemetry should forward only protobuf and server-owned identity when authenticated client exports")]
    [InlineData("logs")]
    [InlineData("metrics")]
    [InlineData("traces")]
    public async Task BrowserTelemetry_Should_ForwardProtobufAndServerIdentity_When_AuthenticatedClientExports(string signal)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var fixture = await Fixture.StartAsync(cancellationToken);
        using var request = CreateRequest(signal, authenticated: true);
        request.Headers.Add("Cookie", "private-cookie=do-not-forward");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "do-not-forward");
        request.Headers.Add("X-Frontend-Service", "forged-server");

        using var response = await fixture.Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var received = fixture.Received.ShouldHaveSingleItem();
        received.Path.ShouldBe("/v1/" + signal);
        received.Body.ShouldBe([1, 2, 3]);
        received.Service.ShouldBe("tactical-heroes-admin-client-development");
        received.Environment.ShouldBe("development");
        received.Cookie.ShouldBeEmpty();
        received.Authorization.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Browser telemetry should reject invalid exports when authentication origin format or size is invalid")]
    [InlineData(false, "logs", "application/x-protobuf", 3, false, HttpStatusCode.Unauthorized)]
    [InlineData(true, "unknown", "application/x-protobuf", 3, false, HttpStatusCode.NotFound)]
    [InlineData(true, "logs", "application/json", 3, false, HttpStatusCode.UnsupportedMediaType)]
    [InlineData(true, "logs", "application/x-protobuf", 1048577, false, HttpStatusCode.RequestEntityTooLarge)]
    [InlineData(true, "logs", "application/x-protobuf", 3, true, HttpStatusCode.Forbidden)]
    public async Task BrowserTelemetry_Should_RejectInvalidExports_When_AuthenticationOriginFormatOrSizeIsInvalid(
        bool authenticated, string signal, string contentType, int size, bool crossOrigin, HttpStatusCode expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var fixture = await Fixture.StartAsync(cancellationToken);
        using var request = CreateRequest(signal, authenticated);
        request.Content = new ByteArrayContent(new byte[size]);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        if (crossOrigin)
        {
            request.Headers.Add("Origin", "https://external.example.test");
        }

        using var response = await fixture.Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expected);
        fixture.Received.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Browser telemetry should reject oversized bodies when content length is unknown")]
    public async Task BrowserTelemetry_Should_RejectOversizedBodies_When_ContentLengthIsUnknown()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var fixture = await Fixture.StartAsync(cancellationToken);
        using var request = CreateRequest("metrics", authenticated: true);
        request.Content = new ChunkedContent(1048577);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        using var response = await fixture.Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.RequestEntityTooLarge);
        fixture.Received.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Browser telemetry should return 429 when a session exceeds its export budget")]
    public async Task BrowserTelemetry_Should_Return429_When_ASessionExceedsItsExportBudget()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var fixture = await Fixture.StartAsync(cancellationToken);
        for (int index = 0; index < 120; index++)
        {
            using var request = CreateRequest("logs", authenticated: true);
            using var response = await fixture.Client.SendAsync(request, cancellationToken);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        using var excessRequest = CreateRequest("logs", authenticated: true);
        using var excessResponse = await fixture.Client.SendAsync(excessRequest, cancellationToken);

        excessResponse.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        fixture.Received.Count.ShouldBe(120);
    }

    private static HttpRequestMessage CreateRequest(string signal, bool authenticated)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "telemetry/v1/" + signal)
        {
            Content = new ByteArrayContent([1, 2, 3])
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");
        if (authenticated)
        {
            request.Headers.Add("X-Test-Authenticated", "true");
        }

        return request;
    }

    private sealed record ReceivedExport(string Path, byte[] Body, string Service, string Environment, string Cookie, string Authorization);

    private sealed class Fixture(WebApplication backend, WebApplication bff, HttpClient client, ConcurrentQueue<ReceivedExport> received) : IAsyncDisposable
    {
        internal HttpClient Client => client;
        internal ConcurrentQueue<ReceivedExport> Received => received;

        internal static async Task<Fixture> StartAsync(CancellationToken cancellationToken, int? bodyLimit = null)
        {
            var received = new ConcurrentQueue<ReceivedExport>();
            var backendBuilder = WebApplication.CreateSlimBuilder();
            backendBuilder.Logging.ClearProviders();
            var backend = backendBuilder.Build();
            backend.Urls.Add("http://127.0.0.1:0");
            backend.MapPost("/v1/{signal}", async (string signal, HttpContext context) =>
            {
                await using var body = new MemoryStream();
                await context.Request.Body.CopyToAsync(body, context.RequestAborted);
                received.Enqueue(new ReceivedExport($"/v1/{signal}", body.ToArray(),
                    context.Request.Headers["X-Frontend-Service"].ToString(),
                    context.Request.Headers["X-Frontend-Environment"].ToString(),
                    context.Request.Headers.Cookie.ToString(), context.Request.Headers.Authorization.ToString()));
                return Results.Bytes([], "application/x-protobuf");
            });
            await backend.StartAsync(cancellationToken);
            var builder = WebApplication.CreateSlimBuilder();
            builder.Logging.ClearProviders();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BrowserTelemetry:CollectorEndpoint"] = backend.Urls.Single(),
                ["BrowserTelemetry:ServiceName"] = "tactical-heroes-admin-client-development",
                ["ASPNETCORE_ENVIRONMENT"] = "Development"
            });
            if (bodyLimit is { } limit)
            {
                builder.Configuration["BrowserTelemetry:MaxRequestBodySize"] = limit.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            builder.Services.AddBrowserTelemetry(builder.Configuration);
            var bff = builder.Build();
            bff.Urls.Add("http://127.0.0.1:0");
            bff.UseStatusCodePagesWithReExecute("/status-code");
            bff.UseRouting();
            bff.Use(async (context, next) =>
            {
                if (context.Request.Headers.ContainsKey("X-Test-Authenticated"))
                {
                    context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "synthetic-user")], "test"));
                }

                await next();
            });
            bff.UseRateLimiter();
            bff.MapBrowserTelemetry();
            bff.MapMethods("/status-code", [HttpMethods.Get, HttpMethods.Post], () => Results.BadRequest());
            await bff.StartAsync(cancellationToken);
            return new Fixture(backend, bff, new HttpClient { BaseAddress = new Uri(bff.Urls.Single()) }, received);
        }

        public async ValueTask DisposeAsync()
        {
            client.Dispose();
            await bff.DisposeAsync();
            await backend.DisposeAsync();
        }
    }

    private sealed class ChunkedContent(int size) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => stream.WriteAsync(new byte[size]).AsTask();

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
