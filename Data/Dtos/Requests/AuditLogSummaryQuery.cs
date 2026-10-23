namespace AuratechBackend.Data.Dtos.Requests;

public sealed class AuditLogSummaryQuery
{
    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }
}
