using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.CampaignTypes;

public interface ICampaignTypeService
{
    Task<Result<IReadOnlyList<CampaignTypeResponse>>> ListAsync(
        bool activeOnly,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignTypeResponse>> GetByIdAsync(
        int id,
        bool activeOnly,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignTypeResponse>> CreateAsync(
        CreateCampaignTypeRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignTypeResponse>> UpdateAsync(
        int id,
        UpdateCampaignTypeRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CampaignTypeResponse>> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default);
}
