using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Responses;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Entities;
using AuratechBackend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.Kyc;

public sealed class CustomerKycService : ICustomerKycService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLogService;

    public CustomerKycService(AppDbContext db, IAuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    public async Task<Result<CustomerKycResponse>> GetForOrganizationAsync(
        int organizationId,
        CancellationToken cancellationToken = default)
    {
        var kyc = await _db.CustomerKycs
            .AsNoTracking()
            .Include(k => k.Organization)
            .FirstOrDefaultAsync(k => k.OrganizationId == organizationId, cancellationToken);

        if (kyc is null)
        {
            return Result<CustomerKycResponse>.NotFound("KYC has not been submitted yet.");
        }

        return Result<CustomerKycResponse>.Success(Map(kyc), "KYC retrieved successfully.");
    }

    public async Task<Result<CustomerKycResponse>> CreateAsync(
        int organizationId,
        int actorUserId,
        SubmitCustomerKycRequest request,
        CancellationToken cancellationToken = default)
    {
        var orgResult = await EnsureCustomerOrganizationAsync(organizationId, cancellationToken);
        if (orgResult.IsFailure)
        {
            return Result<CustomerKycResponse>.Failure(orgResult.Error, orgResult.StatusCode ?? StatusCodes.Status400BadRequest);
        }

        var exists = await _db.CustomerKycs.AnyAsync(k => k.OrganizationId == organizationId, cancellationToken);
        if (exists)
        {
            return Result<CustomerKycResponse>.Conflict("KYC already exists. Use update to resubmit documents.");
        }

        var kyc = new CustomerKyc
        {
            OrganizationId = organizationId,
            TinCertificateUrl = request.TinCertificateUrl.Trim(),
            GovernmentIdUrl = request.GovernmentIdUrl.Trim(),
            Status = KycStatus.Pending,
            RejectionComment = null,
            SubmittedAtUtc = DateTimeOffset.UtcNow
        };

        _db.CustomerKycs.Add(kyc);
        await _db.SaveChangesAsync(cancellationToken);

        await WriteKycAuditAsync(
            organizationId,
            actorUserId,
            AuditActionCodes.KycSubmitted,
            "submitted KYC documents for review.",
            kyc.Id,
            cancellationToken);

        kyc.Organization = orgResult.Value;
        return Result<CustomerKycResponse>.Success(Map(kyc), "KYC submitted successfully.");
    }

    public async Task<Result<CustomerKycResponse>> UpdateAsync(
        int organizationId,
        int actorUserId,
        SubmitCustomerKycRequest request,
        CancellationToken cancellationToken = default)
    {
        var orgResult = await EnsureCustomerOrganizationAsync(organizationId, cancellationToken);
        if (orgResult.IsFailure)
        {
            return Result<CustomerKycResponse>.Failure(orgResult.Error, orgResult.StatusCode ?? StatusCodes.Status400BadRequest);
        }

        var kyc = await _db.CustomerKycs
            .Include(k => k.Organization)
            .FirstOrDefaultAsync(k => k.OrganizationId == organizationId, cancellationToken);

        if (kyc is null)
        {
            return Result<CustomerKycResponse>.NotFound("KYC has not been submitted yet. Use create first.");
        }

        if (kyc.Status == KycStatus.Approved)
        {
            return Result<CustomerKycResponse>.Conflict("Approved KYC cannot be updated.");
        }

        kyc.TinCertificateUrl = request.TinCertificateUrl.Trim();
        kyc.GovernmentIdUrl = request.GovernmentIdUrl.Trim();
        kyc.Status = KycStatus.Pending;
        kyc.RejectionComment = null;
        kyc.SubmittedAtUtc = DateTimeOffset.UtcNow;
        kyc.ReviewedAtUtc = null;
        kyc.ReviewedByUserId = null;

        await _db.SaveChangesAsync(cancellationToken);

        await WriteKycAuditAsync(
            organizationId,
            actorUserId,
            AuditActionCodes.KycUpdated,
            "updated and resubmitted KYC documents.",
            kyc.Id,
            cancellationToken);

        return Result<CustomerKycResponse>.Success(Map(kyc), "KYC updated and resubmitted successfully.");
    }

    public async Task<Result<PagedResult<CustomerKycResponse>>> ListAsync(
        PlatformKycListQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var dbQuery = _db.CustomerKycs
            .AsNoTracking()
            .Include(k => k.Organization)
            .Where(k => k.Organization.Type == OrganizationType.Customer);

        if (query.Status.HasValue)
        {
            dbQuery = dbQuery.Where(k => k.Status == query.Status.Value);
        }

        if (query.From.HasValue)
        {
            dbQuery = dbQuery.Where(k => k.SubmittedAtUtc >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            dbQuery = dbQuery.Where(k => k.SubmittedAtUtc <= query.To.Value);
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderByDescending(k => k.SubmittedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<CustomerKycResponse>
        {
            Items = items.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<CustomerKycResponse>>.Success(result, "KYC submissions retrieved successfully.");
    }

    public async Task<Result<CustomerKycResponse>> GetByOrganizationIdAsync(
        int organizationId,
        CancellationToken cancellationToken = default)
    {
        var orgResult = await EnsureCustomerOrganizationAsync(organizationId, cancellationToken);
        if (orgResult.IsFailure)
        {
            return Result<CustomerKycResponse>.Failure(orgResult.Error, orgResult.StatusCode ?? StatusCodes.Status400BadRequest);
        }

        return await GetForOrganizationAsync(organizationId, cancellationToken);
    }

    public async Task<Result<CustomerKycResponse>> ApproveAsync(
        int organizationId,
        int reviewerUserId,
        CancellationToken cancellationToken = default)
    {
        var orgResult = await EnsureCustomerOrganizationAsync(organizationId, cancellationToken);
        if (orgResult.IsFailure)
        {
            return Result<CustomerKycResponse>.Failure(orgResult.Error, orgResult.StatusCode ?? StatusCodes.Status400BadRequest);
        }

        var kyc = await _db.CustomerKycs
            .Include(k => k.Organization)
            .FirstOrDefaultAsync(k => k.OrganizationId == organizationId, cancellationToken);

        if (kyc is null)
        {
            return Result<CustomerKycResponse>.NotFound("KYC has not been submitted yet.");
        }

        if (kyc.Status != KycStatus.Pending)
        {
            return Result<CustomerKycResponse>.Conflict("Only pending KYC submissions can be approved.");
        }

        kyc.Status = KycStatus.Approved;
        kyc.RejectionComment = null;
        kyc.ReviewedAtUtc = DateTimeOffset.UtcNow;
        kyc.ReviewedByUserId = reviewerUserId;

        await _db.SaveChangesAsync(cancellationToken);

        await WriteKycAuditAsync(
            organizationId,
            reviewerUserId,
            AuditActionCodes.KycApproved,
            "approved KYC submission.",
            kyc.Id,
            cancellationToken);

        return Result<CustomerKycResponse>.Success(Map(kyc), "KYC approved successfully.");
    }

    public async Task<Result<CustomerKycResponse>> RejectAsync(
        int organizationId,
        int reviewerUserId,
        RejectCustomerKycRequest request,
        CancellationToken cancellationToken = default)
    {
        var orgResult = await EnsureCustomerOrganizationAsync(organizationId, cancellationToken);
        if (orgResult.IsFailure)
        {
            return Result<CustomerKycResponse>.Failure(orgResult.Error, orgResult.StatusCode ?? StatusCodes.Status400BadRequest);
        }

        var kyc = await _db.CustomerKycs
            .Include(k => k.Organization)
            .FirstOrDefaultAsync(k => k.OrganizationId == organizationId, cancellationToken);

        if (kyc is null)
        {
            return Result<CustomerKycResponse>.NotFound("KYC has not been submitted yet.");
        }

        if (kyc.Status != KycStatus.Pending)
        {
            return Result<CustomerKycResponse>.Conflict("Only pending KYC submissions can be rejected.");
        }

        kyc.Status = KycStatus.Rejected;
        kyc.RejectionComment = request.Comment.Trim();
        kyc.ReviewedAtUtc = DateTimeOffset.UtcNow;
        kyc.ReviewedByUserId = reviewerUserId;

        await _db.SaveChangesAsync(cancellationToken);

        await WriteKycAuditAsync(
            organizationId,
            reviewerUserId,
            AuditActionCodes.KycRejected,
            "rejected KYC submission.",
            kyc.Id,
            cancellationToken);

        return Result<CustomerKycResponse>.Success(Map(kyc), "KYC rejected successfully.");
    }

    private async Task WriteKycAuditAsync(
        int organizationId,
        int actorUserId,
        string actionCode,
        string actionSummary,
        int kycId,
        CancellationToken cancellationToken)
    {
        var actor = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);
        await _auditLogService.WriteAsync(new AuditLogWriteRequest
        {
            OrganizationId = organizationId,
            ActorUserId = actorUserId,
            ActorDisplayName = AuditActorNames.Format(actor),
            ActionCode = actionCode,
            ActionSummary = actionSummary,
            Category = AuditLogCategory.Security,
            Status = AuditLogStatus.Success,
            ResourceType = "CustomerKyc",
            ResourceId = kycId.ToString(),
            LinkPath = "/dashboard/kyc",
            LinkText = "View KYC"
        }, cancellationToken);
    }

    private async Task<Result<Organization>> EnsureCustomerOrganizationAsync(
        int organizationId,
        CancellationToken cancellationToken)
    {
        var organization = await _db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);

        if (organization is null)
        {
            return Result<Organization>.NotFound("Organization not found.");
        }

        if (organization.Type != OrganizationType.Customer)
        {
            return Result<Organization>.Failure(
                ErrorCodes.Failure,
                "KYC is only available for customer organizations.");
        }

        return Result<Organization>.Success(organization);
    }

    private static CustomerKycResponse Map(CustomerKyc kyc) => new()
    {
        Id = kyc.Id,
        OrganizationId = kyc.OrganizationId,
        OrganizationName = kyc.Organization?.Name ?? string.Empty,
        TinCertificateUrl = kyc.TinCertificateUrl,
        GovernmentIdUrl = kyc.GovernmentIdUrl,
        Status = kyc.Status,
        RejectionComment = kyc.RejectionComment,
        SubmittedAtUtc = kyc.SubmittedAtUtc,
        ReviewedAtUtc = kyc.ReviewedAtUtc,
        ReviewedByUserId = kyc.ReviewedByUserId
    };
}
