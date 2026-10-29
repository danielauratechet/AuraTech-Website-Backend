using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Requests;

public sealed class UpsertConversionTrackingRequest
{
    public ConversionType ConversionType { get; init; }

    public string GoalName { get; init; } = string.Empty;

    public ConversionCountMode CountMode { get; init; } = ConversionCountMode.Every;

    public decimal ConversionValueEtb { get; init; }

    public int ClickThroughWindowDays { get; init; } = 30;

    public ConversionInstallMethod InstallMethod { get; init; } = ConversionInstallMethod.InstallYourself;

    public ConversionTrackingStatus Status { get; init; } = ConversionTrackingStatus.Draft;
}
