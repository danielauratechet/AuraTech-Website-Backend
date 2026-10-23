using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Roles;

public interface IRoleService
{
    Task<Result<IReadOnlyList<PermissionResponse>>> GetPermissionsAsync(CancellationToken cancellationToken = default);

    Result<IReadOnlyList<string>> GetPredefinedRoles(OrganizationType organizationType);

    Task<Result<IReadOnlyList<RoleResponse>>> GetRolesAsync(int organizationId, CancellationToken cancellationToken = default);

    Task<Result<RoleResponse>> CreateRoleAsync(
        int organizationId,
        int actorUserId,
        CreateRoleRequest request,
        CancellationToken cancellationToken = default);
}
