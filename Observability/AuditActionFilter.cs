using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopAPI.Middlewares;
namespace ShopAPI.Observability;

public sealed class AuditActionFilter : IAsyncActionFilter
{
    private readonly IAuditService _auditService;

    public AuditActionFilter(IAuditService auditService) => _auditService = auditService;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        var http = context.HttpContext;
        var routeValues = http.Request.RouteValues;
        var resourceId = routeValues.TryGetValue("id", out var id) ? id?.ToString() : null;
        var controller = routeValues["controller"]?.ToString() ?? "Unknown";
        var action = routeValues["action"]?.ToString() ?? http.Request.Method;
        var statusCode = executed.Exception is null ? http.Response.StatusCode : StatusCodes.Status500InternalServerError;
        var actorId = int.TryParse(http.User.FindFirstValue("sub"), out var parsedId) ? parsedId : (int?)null;
        var role = http.User.FindFirstValue(ClaimTypes.Role);
        var outcome = statusCode is >= 200 and < 400 ? "Success" : "Failure";

        if (outcome == "Success" && HttpMethods.IsGet(http.Request.Method))
            return;

        var details = executed.Exception is null ? null : JsonSerializer.Serialize(new
        {
            Exception = executed.Exception.GetType().Name,
            Message = "Unhandled action exception"
        });
        var correlationId = http.Items[CorrelationIdMiddleware.HeaderName]?.ToString()
            ?? http.TraceIdentifier;

        await _auditService.RecordAsync(new AuditEntry(
            Action: $"{http.Request.Method} {controller}.{action}",
            ResourceType: controller,
            ResourceId: resourceId,
            Outcome: outcome,
            StatusCode: statusCode,
            ActorId: actorId,
            ActorRole: role,
            HttpMethod: http.Request.Method,
            Path: http.Request.Path,
            IpAddress: http.Connection.RemoteIpAddress?.ToString(),
            CorrelationId: correlationId,
            Details: details));
    }
}
