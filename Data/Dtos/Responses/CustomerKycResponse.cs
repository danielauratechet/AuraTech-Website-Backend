using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class CustomerKycResponse
{
    public int Id { get; init; }

    public int OrganizationId { get; init; }

    public string OrganizationName { get; init; } = string.Empty;

    public string TinCertificateUrl { get; init; } = string.Empty;

    public string GovernmentIdUrl { get; init; } = string.Empty;

    public KycStatus Status { get; init; }

    public string? RejectionComment { get; init; }

    public DateTimeOffset SubmittedAtUtc { get; init; }

    public DateTimeOffset? ReviewedAtUtc { get; init; }

    public int? ReviewedByUserId { get; init; }
}
