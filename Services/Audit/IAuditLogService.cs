using AuratechBackend.Core.Responses;
using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Audit;

public interface IAuditLogService
{
    Task WriteAsync(AuditLogWriteRequest request, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<AuditLogResponse>>> ListAsync(
        int organizationId,
        AuditLogListQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<AuditLogSummaryResponse>> GetSummaryAsync(
        int organizationId,
        AuditLogSummaryQuery query,
        CancellationToken cancellationToken = default);
}
