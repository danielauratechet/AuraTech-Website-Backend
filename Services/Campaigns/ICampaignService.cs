using AuratechBackend.Core.Responses;
using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Campaigns;

public interface ICampaignService
{
    Task<Result<CampaignResponse>> CreateAsync(
        int organizationId,
        int userId,
        SaveCampaignStep1Request request,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<CampaignResponse>>> ListAsync(
        int organizationId,
        CustomerCampaignListQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignResponse>> GetByIdAsync(
        int organizationId,
        int campaignId,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignResponse>> SaveStep1Async(
        int organizationId,
        int campaignId,
        SaveCampaignStep1Request request,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignResponse>> SaveStep2Async(
        int organizationId,
        int campaignId,
        SaveCampaignStep2Request request,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignResponse>> SaveStep3Async(
        int organizationId,
        int campaignId,
        SaveCampaignStep3Request request,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignSummaryResponse>> GetSummaryAsync(
        int organizationId,
        int campaignId,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignResponse>> LaunchAsync(
        int organizationId,
        int actorUserId,
        int campaignId,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignResponse>> PauseAsync(
        int organizationId,
        int actorUserId,
        int campaignId,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignResponse>> ResumeAsync(
        int organizationId,
        int actorUserId,
        int campaignId,
        CancellationToken cancellationToken = default);
}
