using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Regions;

public interface IRegionService
{
    Task<Result<IReadOnlyList<RegionResponse>>> ListAsync(
        bool activeOnly,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<Result<RegionResponse>> GetByIdAsync(
        int id,
        bool activeOnly,
        CancellationToken cancellationToken = default);

    Task<Result<RegionResponse>> CreateAsync(
        CreateRegionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<RegionResponse>> UpdateAsync(
        int id,
        UpdateRegionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<RegionResponse>> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default);
}
