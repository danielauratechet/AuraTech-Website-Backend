using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuratechBackend.Data.Configurations;

public sealed class CustomerKycConfiguration : IEntityTypeConfiguration<CustomerKyc>
{
    public void Configure(EntityTypeBuilder<CustomerKyc> builder)
    {
        builder.ToTable("CustomerKycs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.TinCertificateUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(x => x.GovernmentIdUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.RejectionComment)
            .HasMaxLength(2000);

        builder.Property(x => x.SubmittedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.OrganizationId)
            .IsUnique();

        builder.HasOne(x => x.Organization)
            .WithOne()
            .HasForeignKey<CustomerKyc>(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReviewedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Status);
    }
}
