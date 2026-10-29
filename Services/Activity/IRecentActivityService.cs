using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Activity;

public interface IRecentActivityService
{
    Task<Result<IReadOnlyList<RecentActivityItemResponse>>> ListAsync(
        int organizationId,
        int limit,
        CancellationToken cancellationToken = default);
}
