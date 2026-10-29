using AuratechBackend.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuratechBackend.Data;

public class AppDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    int,
    IdentityUserClaim<int>,
    IdentityUserRole<int>,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<CustomerKyc> CustomerKycs => Set<CustomerKyc>();

    public DbSet<CampaignType> CampaignTypes => Set<CampaignType>();

    public DbSet<Region> Regions => Set<Region>();

    public DbSet<Campaign> Campaigns => Set<Campaign>();

    public DbSet<CampaignRegion> CampaignRegions => Set<CampaignRegion>();

    public DbSet<CampaignDailyMetric> CampaignDailyMetrics => Set<CampaignDailyMetric>();

    public DbSet<CampaignDeviceDailyMetric> CampaignDeviceDailyMetrics => Set<CampaignDeviceDailyMetric>();

    public DbSet<CampaignRegionDailyMetric> CampaignRegionDailyMetrics => Set<CampaignRegionDailyMetric>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<OrganizationWallet> OrganizationWallets => Set<OrganizationWallet>();

    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

    public DbSet<ConversionTracking> ConversionTrackings => Set<ConversionTracking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Roles are org-scoped; NormalizedName is "{orgId}:{ROLE}" so Identity uniqueness still holds.
        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.HasIndex(r => r.NormalizedName)
                .IsUnique()
                .HasDatabaseName("RoleNameIndex");
        });
    }
}

