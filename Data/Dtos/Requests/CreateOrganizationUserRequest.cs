namespace AuratechBackend.Data.Dtos.Requests;

public sealed class CreateOrganizationUserRequest
{
    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string? PhoneNumber { get; init; }

    public string RoleName { get; init; } = string.Empty;
}
