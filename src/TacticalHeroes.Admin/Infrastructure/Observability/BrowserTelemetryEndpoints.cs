using System.Net;
using System.Net.Http.Headers;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using OpenTelemetry;

using TacticalHeroes.Admin.Infrastructure.Observability.Options.BrowserTelemetry;

namespace TacticalHeroes.Admin.Infrastructure.Observability;

internal static class BrowserTelemetryEndpoints
{
    private const string ClientName = "BrowserTelemetryCollector";

    internal static bool IsTelemetryRequest(HttpRequest request) => request.Path.StartsWithSegments("/telemetry");

    internal static IServiceCollection AddBrowserTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<BrowserTelemetryOptions>, BrowserTelemetryOptionsValidator>();
        services.AddOptions<BrowserTelemetryOptions>()
            .Bind(configuration.GetSection(BrowserTelemetryOptions.SectionName))
            .ValidateOnStart();
        services.AddHttpClient(ClientName, client =>
        {
            client.BaseAddress = new Uri(configuration.GetRequiredSection("BrowserTelemetry:CollectorEndpoint").Value!);
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
            .WithMetadata(new SkipStatusCodePagesAttribute())
            .AllowAnonymous();
        return endpoints;
    }

    private static async Task<IResult> ForwardAsync(string signal, HttpContext context, IHttpClientFactory clients,
        IOptions<BrowserTelemetryOptions> options)
    {
        var maxBodyBytes = options.Value.MaxRequestBodySize;
        if (ValidateRequest(signal, context, maxBodyBytes) is { } rejection)
        {
            return Results.StatusCode(rejection);
        }

        await using var body = new MemoryStream();
        byte[] buffer = new byte[8192];
        int read;
        while ((read = await context.Request.Body.ReadAsync(buffer, context.RequestAborted)) != 0)
        {
            if (body.Length + read > maxBodyBytes)
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

    private static int? ValidateRequest(string signal, HttpContext context, int maxBodyBytes)
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

        if (request.ContentLength is { } contentLength && contentLength > maxBodyBytes)
        {
            return StatusCodes.Status413PayloadTooLarge;
        }

        return null;
    }
}
