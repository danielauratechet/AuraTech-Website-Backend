using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class AuditLogSummaryResponse
{
    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public int ActionsToday { get; init; }

    public decimal? ActionsTodayChangePercent { get; init; }

    public int ActiveMembersToday { get; init; }

    public decimal TeamCapacityPercent { get; init; }

    public IReadOnlyList<AuditLogMostActiveItemResponse> MostActiveToday { get; init; } = [];

    public IReadOnlyList<AuditLogFrequencyPointResponse> ActivityFrequency { get; init; } = [];

    public AuditLogSystemIntegrityResponse SystemIntegrity { get; init; } = null!;
}

public sealed class AuditLogMostActiveItemResponse
{
    public int? UserId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public int ActionCount { get; init; }
}

public sealed class AuditLogFrequencyPointResponse
{
    public DateOnly Date { get; init; }

    public int Count { get; init; }
}

public sealed class AuditLogSystemIntegrityResponse
{
    public SystemIntegrityStatus Status { get; init; }

    public int FailedCountInRange { get; init; }
}
