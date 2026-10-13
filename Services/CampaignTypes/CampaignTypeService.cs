using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.CampaignTypes;

public sealed class CampaignTypeService : ICampaignTypeService
{
    private readonly AppDbContext _db;

    public CampaignTypeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<CampaignTypeResponse>>> ListAsync(
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _db.CampaignTypes.AsNoTracking();

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        IReadOnlyList<CampaignTypeResponse> items = entities.Select(Map).ToList();
        return Result<IReadOnlyList<CampaignTypeResponse>>.Success(items, "Campaign types retrieved successfully.");
    }

    public async Task<Result<CampaignTypeResponse>> GetByIdAsync(
        int id,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _db.CampaignTypes.AsNoTracking().Where(x => x.Id == id);

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        var entity = await query.FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            return Result<CampaignTypeResponse>.NotFound("Campaign type not found.");
        }

        return Result<CampaignTypeResponse>.Success(Map(entity), "Campaign type retrieved successfully.");
    }

    public async Task<Result<CampaignTypeResponse>> CreateAsync(
        CreateCampaignTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToLowerInvariant();

        var exists = await _db.CampaignTypes.AnyAsync(x => x.Code == code, cancellationToken);
        if (exists)
        {
            return Result<CampaignTypeResponse>.Conflict($"Campaign type code '{code}' already exists.");
        }

        var entity = new CampaignType
        {
            Code = code,
            NameEn = request.NameEn.Trim(),
            NameAm = request.NameAm.Trim(),
            DescriptionEn = request.DescriptionEn.Trim(),
            DescriptionAm = request.DescriptionAm.Trim(),
            IconKey = string.IsNullOrWhiteSpace(request.IconKey) ? null : request.IconKey.Trim(),
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.CampaignTypes.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CampaignTypeResponse>.Success(Map(entity), "Campaign type created successfully.");
    }

    public async Task<Result<CampaignTypeResponse>> UpdateAsync(
        int id,
        UpdateCampaignTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.CampaignTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<CampaignTypeResponse>.NotFound("Campaign type not found.");
        }

        entity.NameEn = request.NameEn.Trim();
        entity.NameAm = request.NameAm.Trim();
        entity.DescriptionEn = request.DescriptionEn.Trim();
        entity.DescriptionAm = request.DescriptionAm.Trim();
        entity.IconKey = string.IsNullOrWhiteSpace(request.IconKey) ? null : request.IconKey.Trim();
        entity.SortOrder = request.SortOrder;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<CampaignTypeResponse>.Success(Map(entity), "Campaign type updated successfully.");
    }

    public async Task<Result<CampaignTypeResponse>> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.CampaignTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<CampaignTypeResponse>.NotFound("Campaign type not found.");
        }

        if (entity.IsActive == isActive)
        {
            return Result<CampaignTypeResponse>.Success(
                Map(entity),
                isActive ? "Campaign type is already active." : "Campaign type is already inactive.");
        }

        entity.IsActive = isActive;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CampaignTypeResponse>.Success(
            Map(entity),
            isActive ? "Campaign type activated successfully." : "Campaign type deactivated successfully.");
    }

    private static CampaignTypeResponse Map(CampaignType entity)
        => new()
        {
            Id = entity.Id,
            Code = entity.Code,
            NameEn = entity.NameEn,
            NameAm = entity.NameAm,
            DescriptionEn = entity.DescriptionEn,
            DescriptionAm = entity.DescriptionAm,
            IconKey = entity.IconKey,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
}
