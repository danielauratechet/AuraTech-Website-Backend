namespace AuratechBackend.Core.Responses;

public class ApiResponse
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public ApiError? Errors { get; init; }

    public string? TraceId { get; init; }

    public static ApiResponse Ok(string? message = null, string? traceId = null) => new()
    {
        Success = true,
        Message = message,
        TraceId = traceId
    };

    public static ApiResponse Fail(ApiError error, string? message = null, string? traceId = null) => new()
    {
        Success = false,
        Message = message ?? error.Message,
        Errors = error,
        TraceId = traceId
    };
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        TraceId = traceId
    };

    public new static ApiResponse<T> Fail(ApiError error, string? message = null, string? traceId = null) => new()
    {
        Success = false,
        Message = message ?? error.Message,
        Errors = error,
        TraceId = traceId
    };
}
