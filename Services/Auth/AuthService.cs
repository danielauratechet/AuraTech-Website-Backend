using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Entities;
using AuratechBackend.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordLinkService _passwordLinkService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IPasswordLinkService passwordLinkService,
        ILogger<AuthService> logger)
    {
        _db = db;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _passwordLinkService = passwordLinkService;
        _logger = logger;
    }

    public async Task<Result> RegisterCustomerAsync(CustomerRegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return Result.Conflict("A user with this email already exists.");
        }

        await PermissionCatalogSeed.EnsureAsync(_db, cancellationToken);

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var organization = new Organization
        {
            Name = request.OrganizationName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Type = OrganizationType.Customer,
            IsActive = true,
            IsSelfManagedCampaign = request.IsSelfManagedCampaign,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _db.Organizations.Add(organization);
        await _db.SaveChangesAsync(cancellationToken);

        await OrganizationRoleBootstrap.EnsureDefaultRolesAsync(_db, organization, cancellationToken);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = request.PhoneNumber.Trim(),
            FirstName = organization.Name,
            LastName = "Owner",
            OrganizationId = organization.Id,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            return Result.Failure(
                ErrorCodes.Failure,
                string.Join(" ", createResult.Errors.Select(e => e.Description)));
        }

        var ownerRole = await _db.Roles.SingleAsync(
            r => r.OrganizationId == organization.Id && r.Name == Constants.Roles.CustomerOwner,
            cancellationToken);

        _db.UserRoles.Add(new IdentityUserRole<int>
        {
            UserId = user.Id,
            RoleId = ownerRole.Id
        });
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var emailResult = await _passwordLinkService.SendSetPasswordLinkAsync(
            user,
            "Set your Auratech password",
            cancellationToken);

        if (emailResult.IsFailure)
        {
            _logger.LogWarning(
                "Customer org {OrgId} created for {Email}, but set-password email failed.",
                organization.Id,
                email);
            return Result.Failure(
                ErrorCodes.Failure,
                "Account was created, but we could not send the set-password email. Use forgot-password to request a new link.");
        }

        _logger.LogInformation("Registered customer org {OrgId} for {Email}", organization.Id, email);

        return Result.Success("Registration successful. Check your email to set your password.");
    }

    public async Task<Result> SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (!PasswordLinkService.TryParseOpaqueToken(request.Token, out var userId, out var resetToken))
        {
            return Result.Failure(ErrorCodes.Failure, "Invalid or expired token.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
        {
            return Result.Failure(ErrorCodes.Failure, "Invalid or expired token.");
        }

        var result = await _userManager.ResetPasswordAsync(user, resetToken, request.Password);
        if (!result.Succeeded)
        {
            var passwordErrors = result.Errors
                .Where(e => e.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Description)
                .ToList();

            if (passwordErrors.Count > 0)
            {
                return Result.Failure(ErrorCodes.Validation, string.Join(" ", passwordErrors));
            }

            return Result.Failure(ErrorCodes.Failure, "Invalid or expired token.");
        }

        user.EmailConfirmed = true;
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        return Result.Success("Password set successfully.");
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
        {
            return Result<LoginResponse>.Failure(ErrorCodes.Unauthorized, "Invalid email or password.", StatusCodes.Status401Unauthorized);
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return Result<LoginResponse>.Failure(ErrorCodes.Unauthorized, "Password has not been set. Check your email for the set-password link.", StatusCodes.Status401Unauthorized);
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Result<LoginResponse>.Failure(ErrorCodes.Unauthorized, "Invalid email or password.", StatusCodes.Status401Unauthorized);
        }

        var (token, expiresAt) = await _jwtTokenService.CreateTokenAsync(user, cancellationToken);
        return Result<LoginResponse>.Success(
            new LoginResponse
            {
                AccessToken = token,
                ExpiresAtUtc = expiresAt
            },
            "Login successful.");
    }

    public async Task<Result<MeResponse>> GetMeAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (user is null)
        {
            return Result<MeResponse>.NotFound("User not found.");
        }

        var organization = await _db.Organizations
            .AsNoTracking()
            .SingleAsync(o => o.Id == user.OrganizationId, cancellationToken);

        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        var roles = await _db.Roles
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name!)
            .ToListAsync(cancellationToken);

        var permissions = await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        KycStatus? kycStatus = null;
        string? rejectionComment = null;

        if (organization.Type == OrganizationType.Customer)
        {
            var kyc = await _db.CustomerKycs
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.OrganizationId == organization.Id, cancellationToken);

            if (kyc is not null)
            {
                kycStatus = kyc.Status;
                rejectionComment = kyc.Status == KycStatus.Rejected ? kyc.RejectionComment : null;
            }
        }

        return Result<MeResponse>.Success(
            new MeResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                OrganizationId = organization.Id,
                OrganizationName = organization.Name,
                OrganizationType = organization.Type,
                IsSelfManagedCampaign = organization.IsSelfManagedCampaign,
                Roles = roles,
                Permissions = permissions,
                KycStatus = kycStatus,
                RejectionComment = rejectionComment
            },
            "Current user retrieved successfully.");
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
        {
            // Avoid account enumeration
            return Result.Success("If an account exists for that email, a reset link has been sent.");
        }

        var emailResult = await _passwordLinkService.SendSetPasswordLinkAsync(
            user,
            "Reset your Auratech password",
            cancellationToken);

        if (emailResult.IsFailure)
        {
            _logger.LogWarning("Forgot-password email failed for {Email}: {Message}", user.Email, emailResult.Error.Message);
        }

        // Avoid account enumeration
        return Result.Success("If an account exists for that email, a reset link has been sent.");
    }

    public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
        {
            return Result.NotFound("User not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return Result.Failure(
                ErrorCodes.Failure,
                string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        return Result.Success("Password changed successfully.");
    }
}
