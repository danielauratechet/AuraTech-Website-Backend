using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Campaigns;

public interface ICampaignAnalyticsService
{
    Task<Result<CampaignAnalyticsOverviewResponse>> GetOverviewAsync(
        int organizationId,
        CampaignAnalyticsOverviewQuery query,
        CancellationToken cancellationToken = default);
}
