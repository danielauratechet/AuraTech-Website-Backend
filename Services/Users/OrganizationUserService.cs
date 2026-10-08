using AuratechBackend.Constants;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Entities;
using AuratechBackend.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.Users;

public sealed class OrganizationUserService : IOrganizationUserService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordLinkService _passwordLinkService;

    public OrganizationUserService(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IPasswordLinkService passwordLinkService)
    {
        _db = db;
        _userManager = userManager;
        _passwordLinkService = passwordLinkService;
    }

    public async Task<Result<IReadOnlyList<UserResponse>>> ListAsync(
        int organizationId,
        CancellationToken cancellationToken = default)
    {
        var users = await _db.Users
            .AsNoTracking()
            .Where(u => u.OrganizationId == organizationId)
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        var responses = new List<UserResponse>();
        foreach (var user in users)
        {
            responses.Add(await MapUserAsync(user, cancellationToken));
        }

        return Result<IReadOnlyList<UserResponse>>.Success(responses, "Users retrieved successfully.");
    }

    public async Task<Result<UserResponse>> CreateAsync(
        int organizationId,
        int actorUserId,
        CreateOrganizationUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return Result<UserResponse>.Conflict("A user with this email already exists.");
        }

        var role = await _db.Roles.FirstOrDefaultAsync(
            r => r.OrganizationId == organizationId && r.Name == request.RoleName,
            cancellationToken);

        if (role is null)
        {
            return Result<UserResponse>.Failure(ErrorCodes.NotFound, "Role not found in this organization.", StatusCodes.Status404NotFound);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            OrganizationId = organizationId,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            return Result<UserResponse>.Failure(
                ErrorCodes.Failure,
                string.Join(" ", createResult.Errors.Select(e => e.Description)));
        }

        _db.UserRoles.Add(new IdentityUserRole<int>
        {
            UserId = user.Id,
            RoleId = role.Id
        });
        await _db.SaveChangesAsync(cancellationToken);

        var emailResult = await _passwordLinkService.SendSetPasswordLinkAsync(
            user,
            "You are invited to Auratech",
            cancellationToken);

        if (emailResult.IsFailure)
        {
            return Result<UserResponse>.Failure(
                ErrorCodes.Failure,
                "User was created, but we could not send the invitation email. Ask them to use forgot-password, or resend later.");
        }

        return Result<UserResponse>.Success(
            await MapUserAsync(user, cancellationToken),
            "User created successfully. An invitation email has been sent.");
    }

    public async Task<Result<UserResponse>> UpdateAsync(
        int organizationId,
        int userId,
        UpdateOrganizationUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Id == userId && u.OrganizationId == organizationId,
            cancellationToken);

        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        var role = await _db.Roles.FirstOrDefaultAsync(
            r => r.OrganizationId == organizationId && r.Name == request.RoleName,
            cancellationToken);

        if (role is null)
        {
            return Result<UserResponse>.Failure(ErrorCodes.NotFound, "Role not found in this organization.", StatusCodes.Status404NotFound);
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var existingRoles = await _db.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync(cancellationToken);
        _db.UserRoles.RemoveRange(existingRoles);
        _db.UserRoles.Add(new IdentityUserRole<int> { UserId = user.Id, RoleId = role.Id });

        await _db.SaveChangesAsync(cancellationToken);
        return Result<UserResponse>.Success(await MapUserAsync(user, cancellationToken), "User updated successfully.");
    }

    public async Task<Result> DeactivateAsync(
        int organizationId,
        int actorUserId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (actorUserId == userId)
        {
            return Result.Failure(ErrorCodes.Failure, "You cannot deactivate your own account.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Id == userId && u.OrganizationId == organizationId,
            cancellationToken);

        if (user is null)
        {
            return Result.NotFound("User not found.");
        }

        user.IsActive = false;
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success("User deactivated successfully.");
    }

    public async Task<Result> ActivateAsync(
        int organizationId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Id == userId && u.OrganizationId == organizationId,
            cancellationToken);

        if (user is null)
        {
            return Result.NotFound("User not found.");
        }

        user.IsActive = true;
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success("User activated successfully.");
    }

    private async Task<UserResponse> MapUserAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == user.Id
            select r.Name!
        ).ToListAsync(cancellationToken);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            PasswordSet = !string.IsNullOrEmpty(user.PasswordHash),
            Roles = roles
        };
    }
}
