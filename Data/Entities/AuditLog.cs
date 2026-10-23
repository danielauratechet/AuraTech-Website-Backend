using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Entities;

public class AuditLog
{
    public long Id { get; set; }

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public int? ActorUserId { get; set; }

    public ApplicationUser? ActorUser { get; set; }

    public string ActorDisplayName { get; set; } = string.Empty;

    public string ActionCode { get; set; } = string.Empty;

    public string ActionSummary { get; set; } = string.Empty;

    public string? HighlightedTarget { get; set; }

    public AuditLogCategory Category { get; set; }

    public AuditLogStatus Status { get; set; } = AuditLogStatus.Success;

    public string? ResourceType { get; set; }

    public string? ResourceId { get; set; }

    public string? LinkPath { get; set; }

    public string? LinkText { get; set; }

    public string? MetadataJson { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
