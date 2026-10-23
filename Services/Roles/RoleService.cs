using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Entities;
using AuratechBackend.Data.Seed;
using AuratechBackend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.Roles;

public sealed class RoleService : IRoleService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLogService;

    public RoleService(AppDbContext db, IAuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    public async Task<Result<IReadOnlyList<PermissionResponse>>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new PermissionResponse
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<PermissionResponse>>.Success(permissions, "Permissions retrieved successfully.");
    }

    public Result<IReadOnlyList<string>> GetPredefinedRoles(OrganizationType organizationType)
    {
        IReadOnlyList<string> roles = organizationType == OrganizationType.Platform
            ? Constants.Roles.PlatformDefaults
            : Constants.Roles.CustomerDefaults;

        return Result<IReadOnlyList<string>>.Success(roles, "Roles retrieved successfully.");
    }

    public async Task<Result<IReadOnlyList<RoleResponse>>> GetRolesAsync(
        int organizationId,
        CancellationToken cancellationToken = default)
    {
        var roles = await _db.Roles
            .AsNoTracking()
            .Where(r => r.OrganizationId == organizationId)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var response = roles.Select(MapRole).ToList();
        return Result<IReadOnlyList<RoleResponse>>.Success(response, "Roles retrieved successfully.");
    }

    public async Task<Result<RoleResponse>> CreateRoleAsync(
        int organizationId,
        int actorUserId,
        CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var organization = await _db.Organizations
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == organizationId, cancellationToken);

        if (organization is null)
        {
            return Result<RoleResponse>.NotFound("Organization not found.");
        }

        var name = request.Name.Trim();
        var exists = await _db.Roles.AnyAsync(
            r => r.OrganizationId == organizationId && r.Name == name,
            cancellationToken);

        if (exists)
        {
            return Result<RoleResponse>.Conflict("A role with this name already exists in the organization.");
        }

        var permissionIds = request.PermissionIds.Distinct().ToList();
        var permissions = await _db.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (permissions.Count != permissionIds.Count)
        {
            return Result<RoleResponse>.Failure(ErrorCodes.Validation, "One or more permission ids are invalid.");
        }

        if (organization.Type == OrganizationType.Customer &&
            permissions.Any(p => p.Code == PermissionCodes.PlatformManage))
        {
            return Result<RoleResponse>.Failure(
                ErrorCodes.Forbidden,
                "Customer roles cannot include platform.manage.",
                StatusCodes.Status403Forbidden);
        }

        var role = new ApplicationRole
        {
            Name = name,
            NormalizedName = IdentitySeed.NormalizeOrgRoleName(organizationId, name),
            OrganizationId = organizationId,
            Description = request.Description?.Trim(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            _db.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        role.RolePermissions = permissions.Select(p => new RolePermission
        {
            RoleId = role.Id,
            PermissionId = p.Id,
            Permission = p
        }).ToList();

        var actor = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);
        await _auditLogService.WriteAsync(new AuditLogWriteRequest
        {
            OrganizationId = organizationId,
            ActorUserId = actorUserId,
            ActorDisplayName = AuditActorNames.Format(actor),
            ActionCode = AuditActionCodes.RoleCreated,
            ActionSummary = $"created \"{role.Name}\" role.",
            HighlightedTarget = role.Name,
            Category = AuditLogCategory.Permissions,
            Status = AuditLogStatus.Success,
            ResourceType = "Role",
            ResourceId = role.Id.ToString(),
            LinkPath = "/dashboard/roles",
            LinkText = "View Roles"
        }, cancellationToken);

        return Result<RoleResponse>.Success(MapRole(role), "Role created successfully.");
    }

    private static RoleResponse MapRole(ApplicationRole role) => new()
    {
        Id = role.Id,
        Name = role.Name ?? string.Empty,
        Description = role.Description,
        OrganizationId = role.OrganizationId,
        Permissions = role.RolePermissions
            .Select(rp => new PermissionResponse
            {
                Id = rp.Permission.Id,
                Code = rp.Permission.Code,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description
            })
            .ToList()
    };
}
