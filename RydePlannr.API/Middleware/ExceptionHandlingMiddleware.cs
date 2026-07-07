using System.Net;
using System.Text.Json;
using RydePlannr.Domain.Exceptions;

namespace RydePlannr.API.Middleware;

public class ExceptionHandlingMiddleware
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
            _logger.LogError(ex, "Neočekivana greška: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException ex       => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException ex => (HttpStatusCode.Unauthorized, ex.Message),
            ForbiddenException ex          => (HttpStatusCode.Forbidden, ex.Message),
            InvalidOperationException ex  => (HttpStatusCode.BadRequest, ex.Message),
            ArgumentException ex          => (HttpStatusCode.BadRequest, ex.Message),
            _                             => (HttpStatusCode.InternalServerError, "Došlo je do interne greške.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            StatusCode = (int)statusCode,
            Message = message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}