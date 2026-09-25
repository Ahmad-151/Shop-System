using Microsoft.Data.SqlClient;
using ShopAPI.Response;
using System.Net;
using System.Text.Json;

namespace ShopAPI.MiddelWares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

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
            catch (SqlException sqlEx)
            {
                _logger.LogWarning(sqlEx, "A database validation error occurred: {Message}", sqlEx.Message);
                await HandleSqlExceptionAsync(context, sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex, _env);
            }
        }

        private static Task HandleSqlExceptionAsync(HttpContext context, SqlException sqlEx)
        {
            (HttpStatusCode statusCode, string message) = sqlEx.Number switch
            {
                >= 50000 and <= 50999 => (HttpStatusCode.BadRequest, sqlEx.Message),
                2601 or 2627 => (HttpStatusCode.Conflict, "This value already exists."),
                547 => (HttpStatusCode.BadRequest, "The request references data that does not exist or violates a data rule."),
                _ => (HttpStatusCode.InternalServerError, "An unexpected database error occurred.")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<string>.FailureResponse(message);
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errors = env.IsDevelopment()
            ? new List<string> { exception.Message, exception.StackTrace ?? string.Empty }
            : new List<string>();

            var response = ApiResponse<string>.FailureResponse
            (
                "An unexpected internal server error occurred. Please try again later.",
                errors
            );

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
