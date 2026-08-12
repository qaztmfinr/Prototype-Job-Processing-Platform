using System.Net;
using System.Text.Json;
using JobProcessingPlatform.Application.Exceptions;

namespace JobProcessingPlatform.API.Middleware;

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
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new { error = exception.Message, timestamp = DateTime.UtcNow };

        return exception switch
        {
            NotFoundException => HandleNotFoundException(context, (NotFoundException)exception, response),
            UnauthorizedException => HandleUnauthorizedException(context, (UnauthorizedException)exception, response),
            ValidationException => HandleValidationException(context, (ValidationException)exception, response),
            JobProcessingException => HandleJobProcessingException(context, (JobProcessingException)exception, response),
            _ => HandleGenericException(context, exception, response)
        };
    }

    private static Task HandleNotFoundException(HttpContext context, NotFoundException ex, object response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleUnauthorizedException(HttpContext context, UnauthorizedException ex, object response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleValidationException(HttpContext context, ValidationException ex, object response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleJobProcessingException(HttpContext context, JobProcessingException ex, object response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleGenericException(HttpContext context, Exception ex, object response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsJsonAsync(response);
    }
}
