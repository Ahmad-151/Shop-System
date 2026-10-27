using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ShopAPI.Middlewares;

public sealed class RequestTelemetryMiddleware
{
    private static readonly Meter Meter = new("ShopAPI.Observability", "1.0.0");
    private static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>(
        "shopapi.http.requests", description: "Number of HTTP requests handled by ShopAPI.");
    private static readonly Counter<long> ErrorCounter = Meter.CreateCounter<long>(
        "shopapi.http.errors", description: "Number of failed HTTP requests.");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>(
        "shopapi.http.duration", unit: "ms", description: "HTTP request duration in milliseconds.");

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTelemetryMiddleware> _logger;

    public RequestTelemetryMiddleware(RequestDelegate next, ILogger<RequestTelemetryMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items[CorrelationIdMiddleware.HeaderName]?.ToString()
            ?? Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        var method = context.Request.Method;
        var path = context.Request.Path.ToString();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TraceId"] = Activity.Current?.TraceId.ToString() ?? string.Empty,
            ["HttpMethod"] = method,
            ["RequestPath"] = path
        }))
        {
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var status = context.Response.StatusCode;
                
                var route = context.GetEndpoint()?.DisplayName ?? "unmatched";
                var tags = new TagList
                {
                    { "method", method },
                    { "route", route },
                    { "status_code", status }
                };
                RequestCounter.Add(1, tags);
                Duration.Record(stopwatch.Elapsed.TotalMilliseconds, tags);
                if (status >= 500) ErrorCounter.Add(1, tags);

                var userId = context.User.FindFirst("sub")?.Value ?? "anonymous";
                _logger.LogInformation(
                    "HTTP request completed with status {StatusCode} in {ElapsedMilliseconds:0.##} ms for user {UserId}",
                    status, stopwatch.Elapsed.TotalMilliseconds, userId);
            }
        }
    }
}
