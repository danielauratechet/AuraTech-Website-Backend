namespace AuratechBackend.Core.Responses;

public sealed class ApiError
{
    public string Code { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public IReadOnlyList<ApiValidationError>? Details { get; init; }
}

public sealed class ApiValidationError
{
    public string PropertyName { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
