using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.ConversionTracking;

public interface IConversionTrackingService
{
    Task<Result<ConversionTrackingResponse>> GetAsync(
        int organizationId,
        CancellationToken cancellationToken = default);

    Task<Result<ConversionTrackingResponse>> UpsertAsync(
        int organizationId,
        UpsertConversionTrackingRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignConversionAnalyticsResponse>> GetCampaignAnalyticsAsync(
        int organizationId,
        int campaignId,
        CampaignConversionAnalyticsQuery query,
        CancellationToken cancellationToken = default);
}
