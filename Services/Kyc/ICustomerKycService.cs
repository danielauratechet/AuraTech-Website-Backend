using AuratechBackend.Core.Responses;
using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Kyc;

public interface ICustomerKycService
{
    Task<Result<CustomerKycResponse>> GetForOrganizationAsync(int organizationId, CancellationToken cancellationToken = default);

    Task<Result<CustomerKycResponse>> CreateAsync(
        int organizationId,
        int actorUserId,
        SubmitCustomerKycRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CustomerKycResponse>> UpdateAsync(
        int organizationId,
        int actorUserId,
        SubmitCustomerKycRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<CustomerKycResponse>>> ListAsync(
        PlatformKycListQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<CustomerKycResponse>> GetByOrganizationIdAsync(
        int organizationId,
        CancellationToken cancellationToken = default);

    Task<Result<CustomerKycResponse>> ApproveAsync(
        int organizationId,
        int reviewerUserId,
        CancellationToken cancellationToken = default);

    Task<Result<CustomerKycResponse>> RejectAsync(
        int organizationId,
        int reviewerUserId,
        RejectCustomerKycRequest request,
        CancellationToken cancellationToken = default);
}
