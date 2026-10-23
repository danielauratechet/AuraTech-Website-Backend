using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Requests;

public sealed class AuditLogListQuery
{
    public int? ActorUserId { get; init; }

    public AuditLogCategory? Category { get; init; }

    public AuditLogStatus? Status { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public string? Q { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
