using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.Regions;

public sealed class RegionService : IRegionService
{
    private readonly AppDbContext _db;

    public RegionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<RegionResponse>>> ListAsync(
        bool activeOnly,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Regions.AsNoTracking();

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Code.Contains(term) ||
                x.NameEn.Contains(term) ||
                x.NameAm.Contains(term));
        }

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        IReadOnlyList<RegionResponse> items = entities.Select(Map).ToList();
        return Result<IReadOnlyList<RegionResponse>>.Success(items, "Regions retrieved successfully.");
    }

    public async Task<Result<RegionResponse>> GetByIdAsync(
        int id,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Regions.AsNoTracking().Where(x => x.Id == id);

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        var entity = await query.FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            return Result<RegionResponse>.NotFound("Region not found.");
        }

        return Result<RegionResponse>.Success(Map(entity), "Region retrieved successfully.");
    }

    public async Task<Result<RegionResponse>> CreateAsync(
        CreateRegionRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        var exists = await _db.Regions.AnyAsync(x => x.Code == code, cancellationToken);
        if (exists)
        {
            return Result<RegionResponse>.Conflict($"Region code '{code}' already exists.");
        }

        var entity = new Region
        {
            Code = code,
            NameEn = request.NameEn.Trim(),
            NameAm = request.NameAm.Trim(),
            EstimatedUsers = request.EstimatedUsers,
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.Regions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<RegionResponse>.Success(Map(entity), "Region created successfully.");
    }

    public async Task<Result<RegionResponse>> UpdateAsync(
        int id,
        UpdateRegionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Regions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<RegionResponse>.NotFound("Region not found.");
        }

        entity.NameEn = request.NameEn.Trim();
        entity.NameAm = request.NameAm.Trim();
        entity.EstimatedUsers = request.EstimatedUsers;
        entity.SortOrder = request.SortOrder;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<RegionResponse>.Success(Map(entity), "Region updated successfully.");
    }

    public async Task<Result<RegionResponse>> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Regions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<RegionResponse>.NotFound("Region not found.");
        }

        if (entity.IsActive == isActive)
        {
            return Result<RegionResponse>.Success(
                Map(entity),
                isActive ? "Region is already active." : "Region is already inactive.");
        }

        entity.IsActive = isActive;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<RegionResponse>.Success(
            Map(entity),
            isActive ? "Region activated successfully." : "Region deactivated successfully.");
    }

    private static RegionResponse Map(Region entity)
        => new()
        {
            Id = entity.Id,
            Code = entity.Code,
            NameEn = entity.NameEn,
            NameAm = entity.NameAm,
            EstimatedUsers = entity.EstimatedUsers,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
}
