using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Dtos.Validators;
using AuratechBackend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.Organizations;

public sealed class OrganizationProfileService : IOrganizationProfileService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLogService;

    public OrganizationProfileService(AppDbContext db, IAuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    public async Task<Result<OrganizationProfileResponse>> GetAsync(
        int organizationId,
        CancellationToken cancellationToken = default)
    {
        var organization = await _db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);

        if (organization is null)
        {
            return Result<OrganizationProfileResponse>.NotFound("Organization not found.");
        }

        return Result<OrganizationProfileResponse>.Success(Map(organization), "Organization profile retrieved successfully.");
    }

    public async Task<Result<OrganizationProfileResponse>> UpdateAsync(
        int organizationId,
        int actorUserId,
        UpdateOrganizationProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var organization = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);

        if (organization is null)
        {
            return Result<OrganizationProfileResponse>.NotFound("Organization not found.");
        }

        organization.Name = request.AgencyName.Trim();
        organization.CompanyEmail = NullIfWhiteSpace(request.ContactEmail);
        organization.Address = NullIfWhiteSpace(request.PhysicalAddress);
        organization.LogoUrl = NullIfWhiteSpace(request.LogoUrl);
        organization.LanguagePreference = NormalizeLanguage(request.LanguagePreference);
        organization.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var actor = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);
        await _auditLogService.WriteAsync(new AuditLogWriteRequest
        {
            OrganizationId = organizationId,
            ActorUserId = actorUserId,
            ActorDisplayName = AuditActorNames.Format(actor),
            ActionCode = AuditActionCodes.OrganizationProfileUpdated,
            ActionSummary = $"updated \"{organization.Name}\" organization profile.",
            HighlightedTarget = organization.Name,
            Category = AuditLogCategory.Settings,
            Status = AuditLogStatus.Success,
            ResourceType = "Organization",
            ResourceId = organization.Id.ToString()
        }, cancellationToken);

        return Result<OrganizationProfileResponse>.Success(Map(organization), "Organization profile updated successfully.");
    }

    private static OrganizationProfileResponse Map(Data.Entities.Organization organization)
        => new()
        {
            AgencyName = organization.Name,
            ContactEmail = organization.CompanyEmail,
            PhysicalAddress = organization.Address,
            LogoUrl = organization.LogoUrl,
            LanguagePreference = string.IsNullOrWhiteSpace(organization.LanguagePreference)
                ? "en-US"
                : organization.LanguagePreference
        };

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeLanguage(string value)
    {
        var trimmed = value.Trim();
        return UpdateOrganizationProfileRequestValidator.AllowedLanguages
            .First(lang => string.Equals(lang, trimmed, StringComparison.OrdinalIgnoreCase));
    }
}
