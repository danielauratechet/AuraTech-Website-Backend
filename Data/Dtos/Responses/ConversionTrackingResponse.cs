using AuratechBackend.Core.Enums;

namespace AuratechBackend.Data.Dtos.Responses;

public sealed class ConversionTrackingResponse
{
    public int Id { get; init; }

    public ConversionType ConversionType { get; init; }

    public string GoalName { get; init; } = string.Empty;

    public ConversionCountMode CountMode { get; init; }

    public decimal ConversionValueEtb { get; init; }

    public int ClickThroughWindowDays { get; init; }

    public ConversionInstallMethod InstallMethod { get; init; }

    public ConversionTrackingStatus Status { get; init; }

    public string TrackingCode { get; init; } = string.Empty;

    public string TagSnippet { get; init; } = string.Empty;

    public string TrackingStatus { get; init; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset? UpdatedAtUtc { get; init; }
}
