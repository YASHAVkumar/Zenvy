namespace zenvy.shared.Reponses;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public object? Errors { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public DateTime TimestampUtc { get; init; } = DateTime.Now;

    public static ApiResponse<T> Succeeded(T? data, string message, string traceId) => new()
    {
        Success = true,
        Message = message,
        Data = data,
        TraceId = traceId
    };

    public static ApiResponse<T> Failed(string message, string traceId, object? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors,
        TraceId = traceId
    };
}
