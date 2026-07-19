using System.Net;
using System.Text.Json;
using HRPerformance.Application.Common;

namespace HRPerformance.API.Middleware;
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger) { _next = next; _logger = logger; }
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            var message = DatabaseErrorHelper.IsDatabaseError(ex)
                ? DatabaseErrorHelper.GetPersianMessage(ex)
                : "خطای داخلی سرور";
            var status = DatabaseErrorHelper.IsDatabaseError(ex)
                ? HttpStatusCode.ServiceUnavailable
                : HttpStatusCode.InternalServerError;
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { success = false, message }));
        }
    }
}
