namespace AuratechBackend.Data.Dtos.Responses;

public sealed class RoleResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int OrganizationId { get; init; }

    public IReadOnlyList<PermissionResponse> Permissions { get; init; } = [];
}
