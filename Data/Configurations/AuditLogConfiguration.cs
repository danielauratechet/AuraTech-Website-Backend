using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuratechBackend.Data.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.ActorDisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ActionCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ActionSummary)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.HighlightedTarget)
            .HasMaxLength(200);

        builder.Property(x => x.Category)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ResourceType)
            .HasMaxLength(100);

        builder.Property(x => x.ResourceId)
            .HasMaxLength(100);

        builder.Property(x => x.LinkPath)
            .HasMaxLength(500);

        builder.Property(x => x.LinkText)
            .HasMaxLength(100);

        builder.Property(x => x.MetadataJson)
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ActorUser)
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.OrganizationId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.OrganizationId, x.Category });
        builder.HasIndex(x => new { x.OrganizationId, x.ActorUserId });
    }
}
