using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class OrganizationResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public OrganizationType Type { get; init; }

    public bool IsActive { get; init; }
}
