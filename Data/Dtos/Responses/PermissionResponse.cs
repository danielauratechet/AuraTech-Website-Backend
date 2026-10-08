namespace AuratechBackend.Data.Dtos.Responses;

public sealed class PermissionResponse
{
    public int Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}
