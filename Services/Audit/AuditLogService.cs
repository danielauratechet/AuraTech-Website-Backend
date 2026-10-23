using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Responses;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.Audit;

public sealed class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(AppDbContext db, ILogger<AuditLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task WriteAsync(AuditLogWriteRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.OrganizationId <= 0
                || string.IsNullOrWhiteSpace(request.ActionCode)
                || string.IsNullOrWhiteSpace(request.ActionSummary))
            {
                _logger.LogWarning(
                    "Skipping audit write due to incomplete payload. Org={OrganizationId}, Code={ActionCode}",
                    request.OrganizationId,
                    request.ActionCode);
                return;
            }

            var displayName = string.IsNullOrWhiteSpace(request.ActorDisplayName)
                ? (request.ActorUserId is null ? "System" : "Unknown user")
                : request.ActorDisplayName.Trim();

            _db.AuditLogs.Add(new AuditLog
            {
                OrganizationId = request.OrganizationId,
                ActorUserId = request.ActorUserId,
                ActorDisplayName = displayName,
                ActionCode = request.ActionCode.Trim(),
                ActionSummary = request.ActionSummary.Trim(),
                HighlightedTarget = string.IsNullOrWhiteSpace(request.HighlightedTarget)
                    ? null
                    : request.HighlightedTarget.Trim(),
                Category = request.Category,
                Status = request.Status,
                ResourceType = string.IsNullOrWhiteSpace(request.ResourceType) ? null : request.ResourceType.Trim(),
                ResourceId = string.IsNullOrWhiteSpace(request.ResourceId) ? null : request.ResourceId.Trim(),
                LinkPath = string.IsNullOrWhiteSpace(request.LinkPath) ? null : request.LinkPath.Trim(),
                LinkText = string.IsNullOrWhiteSpace(request.LinkText) ? null : request.LinkText.Trim(),
                MetadataJson = request.MetadataJson,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });

            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to write audit log. Org={OrganizationId}, Code={ActionCode}",
                request.OrganizationId,
                request.ActionCode);
        }
    }

    public async Task<Result<PagedResult<AuditLogResponse>>> ListAsync(
        int organizationId,
        AuditLogListQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var dbQuery = _db.AuditLogs.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId);

        if (query.ActorUserId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.ActorUserId == query.ActorUserId.Value);
        }

        if (query.Category.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Category == query.Category.Value);
        }

        if (query.Status.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.From.HasValue)
        {
            var fromUtc = query.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            dbQuery = dbQuery.Where(x => x.CreatedAtUtc >= fromUtc);
        }

        if (query.To.HasValue)
        {
            var toExclusive = query.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            dbQuery = dbQuery.Where(x => x.CreatedAtUtc < toExclusive);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            dbQuery = dbQuery.Where(x =>
                x.ActionSummary.Contains(term) ||
                x.ActorDisplayName.Contains(term) ||
                (x.HighlightedTarget != null && x.HighlightedTarget.Contains(term)));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var entities = await dbQuery
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result<PagedResult<AuditLogResponse>>.Success(new PagedResult<AuditLogResponse>
        {
            Items = entities.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        }, "Audit logs retrieved successfully.");
    }

    public async Task<Result<AuditLogSummaryResponse>> GetSummaryAsync(
        int organizationId,
        AuditLogSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = query.To ?? today;
        var from = query.From ?? to.AddDays(-29);

        if (from > to)
        {
            return Result<AuditLogSummaryResponse>.ValidationFailure(
            [
                new ValidationError(nameof(query.From), "From must be on or before To.")
            ]);
        }

        if (to.DayNumber - from.DayNumber > 366)
        {
            return Result<AuditLogSummaryResponse>.ValidationFailure(
            [
                new ValidationError(nameof(query.To), "Date range cannot exceed 366 days.")
            ]);
        }

        var rangeStartUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var rangeEndExclusiveUtc = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var todayStartUtc = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var tomorrowStartUtc = today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var rangeLogs = _db.AuditLogs.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                        && x.CreatedAtUtc >= rangeStartUtc
                        && x.CreatedAtUtc < rangeEndExclusiveUtc);

        var actionsToday = await _db.AuditLogs.AsNoTracking()
            .CountAsync(
                x => x.OrganizationId == organizationId
                     && x.CreatedAtUtc >= todayStartUtc
                     && x.CreatedAtUtc < tomorrowStartUtc,
                cancellationToken);

        var dayCount = to.DayNumber - from.DayNumber + 1;
        var totalInRange = await rangeLogs.CountAsync(cancellationToken);
        var dailyAverage = dayCount <= 0 ? 0m : (decimal)totalInRange / dayCount;
        decimal? changePercent = null;
        if (dailyAverage > 0)
        {
            changePercent = Math.Round((actionsToday - dailyAverage) * 100m / dailyAverage, 2);
        }
        else if (actionsToday > 0)
        {
            changePercent = 100m;
        }

        var activeMembersToday = await _db.AuditLogs.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                        && x.CreatedAtUtc >= todayStartUtc
                        && x.CreatedAtUtc < tomorrowStartUtc
                        && x.ActorUserId != null)
            .Select(x => x.ActorUserId!.Value)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalActiveUsers = await _db.Users.AsNoTracking()
            .CountAsync(u => u.OrganizationId == organizationId && u.IsActive, cancellationToken);

        var teamCapacityPercent = totalActiveUsers == 0
            ? 0m
            : Math.Round(activeMembersToday * 100m / totalActiveUsers, 2);

        var mostActiveToday = await _db.AuditLogs.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                        && x.CreatedAtUtc >= todayStartUtc
                        && x.CreatedAtUtc < tomorrowStartUtc
                        && x.ActorUserId != null)
            .GroupBy(x => new { x.ActorUserId, x.ActorDisplayName })
            .Select(g => new AuditLogMostActiveItemResponse
            {
                UserId = g.Key.ActorUserId,
                DisplayName = g.Key.ActorDisplayName,
                ActionCount = g.Count()
            })
            .OrderByDescending(x => x.ActionCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        var createdAtList = await rangeLogs
            .Select(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var frequencyByDate = createdAtList
            .GroupBy(x => DateOnly.FromDateTime(x.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Count());
        var activityFrequency = new List<AuditLogFrequencyPointResponse>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            activityFrequency.Add(new AuditLogFrequencyPointResponse
            {
                Date = d,
                Count = frequencyByDate.GetValueOrDefault(d)
            });
        }

        var failedCount = await rangeLogs.CountAsync(x => x.Status == AuditLogStatus.Failed, cancellationToken);
        var integrityStatus = failedCount switch
        {
            0 => SystemIntegrityStatus.Healthy,
            <= 5 => SystemIntegrityStatus.Warning,
            _ => SystemIntegrityStatus.Critical
        };

        return Result<AuditLogSummaryResponse>.Success(new AuditLogSummaryResponse
        {
            From = from,
            To = to,
            ActionsToday = actionsToday,
            ActionsTodayChangePercent = changePercent,
            ActiveMembersToday = activeMembersToday,
            TeamCapacityPercent = teamCapacityPercent,
            MostActiveToday = mostActiveToday,
            ActivityFrequency = activityFrequency,
            SystemIntegrity = new AuditLogSystemIntegrityResponse
            {
                Status = integrityStatus,
                FailedCountInRange = failedCount
            }
        }, "Audit log summary retrieved successfully.");
    }

    private static AuditLogResponse Map(AuditLog entity)
        => new()
        {
            Id = entity.Id,
            ActorUserId = entity.ActorUserId,
            ActorDisplayName = entity.ActorDisplayName,
            ActionCode = entity.ActionCode,
            ActionSummary = entity.ActionSummary,
            HighlightedTarget = entity.HighlightedTarget,
            Category = entity.Category,
            Status = entity.Status,
            ResourceType = entity.ResourceType,
            ResourceId = entity.ResourceId,
            LinkText = entity.LinkText,
            LinkPath = entity.LinkPath,
            IsSystem = entity.ActorUserId is null,
            CreatedAtUtc = entity.CreatedAtUtc
        };
}
