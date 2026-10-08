using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Entities;

public class CustomerKyc
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string TinCertificateUrl { get; set; } = string.Empty;

    public string GovernmentIdUrl { get; set; } = string.Empty;

    public KycStatus Status { get; set; } = KycStatus.Pending;

    public string? RejectionComment { get; set; }

    public DateTimeOffset SubmittedAtUtc { get; set; }

    public DateTimeOffset? ReviewedAtUtc { get; set; }

    public int? ReviewedByUserId { get; set; }

    public ApplicationUser? ReviewedByUser { get; set; }
}
