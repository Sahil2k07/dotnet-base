namespace DotnetBase.Contract.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }

    public required string Message { get; set; }

    public T? Data { get; set; }

    public object? Errors { get; set; }
}
