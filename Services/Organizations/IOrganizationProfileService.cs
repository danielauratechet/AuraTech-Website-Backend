using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Organizations;

public interface IOrganizationProfileService
{
    Task<Result<OrganizationProfileResponse>> GetAsync(
        int organizationId,
        CancellationToken cancellationToken = default);

    Task<Result<OrganizationProfileResponse>> UpdateAsync(
        int organizationId,
        int actorUserId,
        UpdateOrganizationProfileRequest request,
        CancellationToken cancellationToken = default);
}
