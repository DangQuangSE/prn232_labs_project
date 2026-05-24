using System.Text.Json;
using PRN232.LMSSystem.Services.Exceptions;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        string message;

        switch (exception)
        {
            case NotFoundException notFound:
                statusCode = 404;
                message = notFound.Message;
                _logger.LogWarning("[404 Not Found] {Method} {Path} — {Message}",
                    context.Request.Method, context.Request.Path, message);
                break;

            case BadRequestException badRequest:
                statusCode = 400;
                message = badRequest.Message;
                _logger.LogWarning("[400 Bad Request] {Method} {Path} — {Message}",
                    context.Request.Method, context.Request.Path, message);
                break;

            default:
                statusCode = 500;
                message = "An unexpected error occurred. Please try again later.";
                _logger.LogError(exception,
                    "[500 Internal Server Error] {Method} {Path} — {ExceptionType}: {ExceptionMessage}",
                    context.Request.Method, context.Request.Path,
                    exception.GetType().Name, exception.Message);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.ErrorResponse(message);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
