using System.Diagnostics;
using AuratechBackend.Constants;
using AuratechBackend.Core.Responses;

namespace AuratechBackend.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = ApiResponse.Fail(
                new ApiError
                {
                    Code = ErrorCodes.Unexpected,
                    Message = "An unexpected error occurred."
                },
                traceId: Activity.Current?.Id ?? context.TraceIdentifier);

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
