using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Users;

public interface IOrganizationUserService
{
    Task<Result<IReadOnlyList<UserResponse>>> ListAsync(int organizationId, CancellationToken cancellationToken = default);

    Task<Result<UserResponse>> CreateAsync(
        int organizationId,
        int actorUserId,
        CreateOrganizationUserRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<UserResponse>> UpdateAsync(
        int organizationId,
        int userId,
        UpdateOrganizationUserRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeactivateAsync(int organizationId, int actorUserId, int userId, CancellationToken cancellationToken = default);

    Task<Result> ActivateAsync(int organizationId, int userId, CancellationToken cancellationToken = default);
}
