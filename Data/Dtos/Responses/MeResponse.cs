using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class MeResponse
{
    public int Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public int OrganizationId { get; init; }

    public string OrganizationName { get; init; } = string.Empty;

    public OrganizationType OrganizationType { get; init; }

    public bool IsSelfManagedCampaign { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = [];

    public IReadOnlyList<string> Permissions { get; init; } = [];

    public KycStatus? KycStatus { get; init; }

    public string? RejectionComment { get; init; }
}
