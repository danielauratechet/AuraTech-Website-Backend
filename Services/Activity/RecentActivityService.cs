using AuratechBackend.Constants;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Services.Activity;

public sealed class RecentActivityService : IRecentActivityService
{
    private const int DefaultLimit = 5;
    private const int MaxLimit = 20;

    private readonly AppDbContext _db;

    public RecentActivityService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<RecentActivityItemResponse>>> ListAsync(
        int organizationId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var take = limit is < 1 or > MaxLimit ? DefaultLimit : limit;

        var logs = await _db.AuditLogs.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        IReadOnlyList<RecentActivityItemResponse> items = logs.Select(Map).ToList();
        return Result<IReadOnlyList<RecentActivityItemResponse>>.Success(items, "Recent activity retrieved successfully.");
    }

    private static RecentActivityItemResponse Map(AuditLog log)
        => new()
        {
            Id = log.Id,
            ActionCode = log.ActionCode,
            Category = log.Category,
            Title = BuildTitle(log),
            OccurredAtUtc = log.CreatedAtUtc
        };

    private static string BuildTitle(AuditLog log)
    {
        var name = log.HighlightedTarget;
        if (!string.IsNullOrWhiteSpace(name))
        {
            var quoted = $"'{name}'";
            var titled = log.ActionCode switch
            {
                AuditActionCodes.CampaignLaunched => $"Campaign {quoted} launched",
                AuditActionCodes.CampaignUpdated => $"Campaign {quoted} updated",
                AuditActionCodes.CampaignPaused => $"Campaign {quoted} paused",
                AuditActionCodes.CampaignResumed => $"Campaign {quoted} resumed",
                _ => null
            };

            if (titled is not null)
            {
                return titled;
            }
        }

        return log.ActionCode switch
        {
            AuditActionCodes.KycApproved => "KYC approved",
            AuditActionCodes.KycRejected => "KYC rejected",
            AuditActionCodes.KycSubmitted => "KYC submitted",
            AuditActionCodes.BillingDeposited => log.ActionSummary,
            _ => log.ActionSummary
        };
    }
}
