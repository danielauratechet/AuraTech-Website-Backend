using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Requests;

public sealed class CustomerCampaignListQuery
{
    public CampaignStatus? Status { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
