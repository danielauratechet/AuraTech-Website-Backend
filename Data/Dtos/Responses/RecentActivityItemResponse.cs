using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class RecentActivityItemResponse
{
    public long Id { get; init; }

    public string ActionCode { get; init; } = string.Empty;

    public AuditLogCategory Category { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; init; }
}
