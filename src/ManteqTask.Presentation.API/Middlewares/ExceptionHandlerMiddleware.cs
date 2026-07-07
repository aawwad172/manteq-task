using System.ComponentModel.DataAnnotations;
using System.Text.Json;

using ManteqTask.Domain.Exceptions;
using ManteqTask.Presentation.API.Models;


namespace ManteqTask.Presentation.API.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // NOTE: Expected/business errors (not found, conflict, auth, validation, ...) are now
        // returned as domain Results and mapped to HTTP by ResultExtensions. This middleware only
        // handles infrastructure/programmer errors that are genuinely exceptional.
        try
        {
            await _next(context);
        }
        catch (EnvironmentVariableNotSetException ex)
        {
            _logger.LogError("EnvironmentVariableNotSetException occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, "ENV_VAR_MISSING", ex.Message, StatusCodes.Status500InternalServerError);
        }
        catch (ValidationException ex)
        {
            _logger.LogError("ValidationException occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, "VALIDATION_ERROR", ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, "UNEXPECTED_ERROR", "An unexpected error occurred.", StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string errorCode, string message, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        ApiResponse<string> response = ApiResponse<string>.ErrorResponse(message, errorCode, statusCode);
        string result = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(result);
    }
}
