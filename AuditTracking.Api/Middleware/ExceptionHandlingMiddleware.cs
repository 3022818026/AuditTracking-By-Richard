using AuditTracking.Api.Common;
using System.Net;
using System.Text.Json;

namespace AuditTracking.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var traceId = context.TraceIdentifier;

        _logger.LogError(
            exception,
            "发生未处理异常。TraceId: {TraceId}，请求路径: {Path}",
            traceId,
            context.Request.Path);

        if (context.Response.HasStarted)
        {
            throw exception;
        }

        context.Response.Clear();

        context.Response.StatusCode =
            (int)HttpStatusCode.InternalServerError;

        context.Response.ContentType =
            "application/json; charset=utf-8";

        var response = ApiResponse.Fail(
            message: "服务器内部错误，请稍后重试",
            traceId: traceId);

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase
            });

        await context.Response.WriteAsync(json);
    }
}