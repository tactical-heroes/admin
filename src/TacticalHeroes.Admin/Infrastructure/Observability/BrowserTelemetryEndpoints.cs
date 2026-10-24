using System.Net;
using System.Net.Http.Headers;
using System.Threading.RateLimiting;

using OpenTelemetry;

namespace TacticalHeroes.Admin.Infrastructure.Observability;

internal static class BrowserTelemetryEndpoints
{
    private const string ClientName = "BrowserTelemetryCollector";
    private const int MaxBodyBytes = 64 * 1024;

    internal static bool IsTelemetryRequest(HttpRequest request) => request.Path.StartsWithSegments("/telemetry");

    internal static IServiceCollection AddBrowserTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient(ClientName, client =>
        {
            client.BaseAddress = new Uri(configuration["BrowserTelemetry:CollectorEndpoint"] ??
                "http://otel-collector.observability.svc.cluster.local:4319/");
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.Add("X-Frontend-Service", configuration["BrowserTelemetry:ServiceName"] ?? "tactical-heroes-admin-client");
            client.DefaultRequestHeaders.Add("X-Frontend-Environment", (configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production").ToLowerInvariant());
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false
        });
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(ClientName, context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.FindFirst("sub")?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        });
        return services;
    }

    internal static IEndpointRouteBuilder MapBrowserTelemetry(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/telemetry/v1/{signal}", ForwardAsync)
            .RequireRateLimiting(ClientName)
            .AllowAnonymous();
        return endpoints;
    }

    private static async Task<IResult> ForwardAsync(string signal, HttpContext context, IHttpClientFactory clients)
    {
        if (ValidateRequest(signal, context) is { } rejection)
        {
            return Results.StatusCode(rejection);
        }

        await using var body = new MemoryStream();
        byte[] buffer = new byte[8192];
        int read;
        while ((read = await context.Request.Body.ReadAsync(buffer, context.RequestAborted)) != 0)
        {
            if (body.Length + read > MaxBodyBytes)
            {
                return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
            }

            await body.WriteAsync(buffer.AsMemory(0, read), context.RequestAborted);
        }

        using var forwarding = new HttpRequestMessage(HttpMethod.Post, "v1/" + signal)
        {
            Content = new ByteArrayContent(body.ToArray())
        };
        forwarding.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");
        using var suppression = SuppressInstrumentationScope.Begin();
        try
        {
            using var client = clients.CreateClient(ClientName);
            using var response = await client.SendAsync(forwarding,
                HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
            return response.IsSuccessStatusCode
                ? Results.Bytes([], "application/x-protobuf")
                : Results.StatusCode((int)HttpStatusCode.BadGateway);
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException)
        {
            return Results.StatusCode((int)HttpStatusCode.ServiceUnavailable);
        }
    }

    private static int? ValidateRequest(string signal, HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return StatusCodes.Status401Unauthorized;
        }

        if (signal is not ("logs" or "metrics" or "traces"))
        {
            return StatusCodes.Status404NotFound;
        }

        var request = context.Request;
        if ((request.Headers["Sec-Fetch-Site"].Count > 0 && request.Headers["Sec-Fetch-Site"] != "same-origin") ||
            (request.Headers.Origin.Count > 0 && !string.Equals(request.Headers.Origin.ToString(),
                $"{request.Scheme}://{request.Host}", StringComparison.OrdinalIgnoreCase)))
        {
            return StatusCodes.Status403Forbidden;
        }

        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var contentType) ||
            contentType.MediaType != "application/x-protobuf" || request.Headers.ContentEncoding.Count != 0)
        {
            return StatusCodes.Status415UnsupportedMediaType;
        }

        if (request.ContentLength is > MaxBodyBytes)
        {
            return StatusCodes.Status413PayloadTooLarge;
        }

        return null;
    }
}
