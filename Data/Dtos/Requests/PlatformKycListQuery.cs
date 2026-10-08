using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Requests;

public sealed class PlatformKycListQuery
{
    public KycStatus? Status { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }
}
