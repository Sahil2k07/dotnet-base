using System.Text;
using System.Text.Json;
using DotnetBase.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotnetBase.Shared.Middleware;

public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger
    )
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
        catch (AppException ex)
        {
            await HandleExceptionAsync(context, ex, ex.StatusCode);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        int statusCode
    )
    {
        await LogExceptionAsync(context, exception);

        await WriteResponseAsync(
            context,
            statusCode,
            exception is AppException ? exception.Message : "An unexpected error occurred."
        );
    }

    private static async Task WriteResponseAsync(
        HttpContext context,
        int statusCode,
        string message
    )
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(
            new
            {
                Success = false,
                Message = message,
                Data = (object?)null,
            }
        );
    }

    private async Task LogExceptionAsync(HttpContext context, Exception exception)
    {
        var requestBody = await ReadRequestBodyAsync(context);

        _logger.LogError(
            exception,
            "Unhandled exception. Method: {Method}, Path: {Path}, Query: {@Query}, Body: {@Body}",
            context.Request.Method,
            context.Request.Path,
            context.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
            requestBody
        );
    }

    private static async Task<object?> ReadRequestBodyAsync(HttpContext context)
    {
        if (context.Request.ContentLength is null || context.Request.ContentLength == 0)
        {
            return null;
        }

        context.Request.EnableBuffering();

        context.Request.Body.Position = 0;

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true
        );

        var body = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch
        {
            return body;
        }
    }
}
