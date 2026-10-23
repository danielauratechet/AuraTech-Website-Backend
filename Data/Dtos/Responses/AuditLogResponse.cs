using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class AuditLogResponse
{
    public long Id { get; init; }

    public int? ActorUserId { get; init; }

    public string ActorDisplayName { get; init; } = string.Empty;

    public string ActionCode { get; init; } = string.Empty;

    public string ActionSummary { get; init; } = string.Empty;

    public string? HighlightedTarget { get; init; }

    public AuditLogCategory Category { get; init; }

    public AuditLogStatus Status { get; init; }

    public string? ResourceType { get; init; }

    public string? ResourceId { get; init; }

    public string? LinkText { get; init; }

    public string? LinkPath { get; init; }

    public bool IsSystem { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }
}
