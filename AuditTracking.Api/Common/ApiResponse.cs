namespace AuditTracking.Api.Common;

/// <summary>
/// 统一 API 返回结构。
/// </summary>
/// <typeparam name="T">返回的数据类型。</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// 请求是否成功。
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 返回消息。
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 返回的数据。
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// 错误详情。
    /// </summary>
    public object? Errors { get; init; }

    /// <summary>
    /// 请求跟踪编号。
    /// </summary>
    public string? TraceId { get; init; }
}

/// <summary>
/// 统一 API 响应创建工具。
/// </summary>
public static class ApiResponse
{
    /// <summary>
    /// 创建带数据的成功响应。
    /// </summary>
    public static ApiResponse<T> Ok<T>(
        T data,
        string message = "操作成功")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null,
            TraceId = null
        };
    }

    /// <summary>
    /// 创建不带数据的成功响应。
    /// </summary>
    public static ApiResponse<object?> Ok(
        string message = "操作成功")
    {
        return new ApiResponse<object?>
        {
            Success = true,
            Message = message,
            Data = null,
            Errors = null,
            TraceId = null
        };
    }

    /// <summary>
    /// 创建失败响应。
    /// </summary>
    public static ApiResponse<object?> Fail(
        string message,
        object? errors = null,
        string? traceId = null)
    {
        return new ApiResponse<object?>
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors,
            TraceId = traceId
        };
    }
}