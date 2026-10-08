namespace AuratechBackend.Data.Dtos.Requests;

public sealed class SubmitCustomerKycRequest
{
    public string TinCertificateUrl { get; init; } = string.Empty;

    public string GovernmentIdUrl { get; init; } = string.Empty;
}
