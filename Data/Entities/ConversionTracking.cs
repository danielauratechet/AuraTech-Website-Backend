using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Entities;

public class ConversionTracking
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public ConversionType ConversionType { get; set; }

    public string GoalName { get; set; } = string.Empty;

    public ConversionCountMode CountMode { get; set; } = ConversionCountMode.Every;

    public decimal ConversionValueEtb { get; set; }

    public int ClickThroughWindowDays { get; set; } = 30;

    public ConversionInstallMethod InstallMethod { get; set; } = ConversionInstallMethod.InstallYourself;

    public ConversionTrackingStatus Status { get; set; } = ConversionTrackingStatus.Draft;

    public string TrackingCode { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
