using Microsoft.Data.SqlClient;
using ShopAPI.Response;
using System.Net;
using System.Text.Json;

namespace ShopAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        public ExceptionHandlingMiddleware
            (
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env
            )
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogDebug(
                    "Request was cancelled by the client. CorrelationId: {CorrelationId}",
                    GetCorrelationId(context));

                if (!context.Response.HasStarted)
                    context.Response.StatusCode = 499;
            }
            catch (SqlException sqlEx)
            {
                var isBusinessError = sqlEx.Number is >= 50000 and <= 52999 or 2601 or 2627 or 547;
                _logger.Log(
                    isBusinessError ? LogLevel.Warning : LogLevel.Error,
                    sqlEx,
                    "A database error occurred (number {SqlErrorNumber}). CorrelationId: {CorrelationId}",
                    sqlEx.Number,
                    GetCorrelationId(context));

                if (context.Response.HasStarted) throw;
                await HandleSqlExceptionAsync(context, sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unhandled exception occurred. CorrelationId: {CorrelationId}",
                    GetCorrelationId(context));

                if (context.Response.HasStarted) throw;
                await HandleExceptionAsync(context, ex, _env);
            }
        }

        private static string? GetCorrelationId(HttpContext context) =>
         context.Items[CorrelationIdMiddleware.HeaderName]?.ToString();

        private static Task WriteFailureAsync(
            HttpContext context, HttpStatusCode statusCode, string message, List<string>? errors = null)
        {
            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<string>.FailureResponse(
                message, errors, GetCorrelationId(context));

            return context.Response.WriteAsJsonAsync(response, _jsonOptions);
        }

        private static Task HandleSqlExceptionAsync(HttpContext context, SqlException sqlEx)
        {
            (HttpStatusCode statusCode, string message) = sqlEx.Number switch
            {
                50005 or 52012 => (HttpStatusCode.Conflict, sqlEx.Message),
                >= 50000 and <= 52999 => (HttpStatusCode.BadRequest, sqlEx.Message),
                2601 or 2627 => (HttpStatusCode.Conflict, "This value already exists."),
                547 => (HttpStatusCode.BadRequest, "The request references data that does not exist or violates a data rule."),
                _ => (HttpStatusCode.InternalServerError, "An unexpected database error occurred.")
            };

            return WriteFailureAsync(context, statusCode, message);
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env)
        {
            var errors = env.IsDevelopment()
                ? new List<string> { exception.Message, exception.StackTrace ?? string.Empty }
                : new List<string>();

            return WriteFailureAsync(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected internal server error occurred. Please try again later.",
                errors);
        }
    }
}
