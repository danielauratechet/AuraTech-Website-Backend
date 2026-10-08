namespace AuratechBackend.Data.Dtos.Responses;

public sealed class UserResponse
{
    public int Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; }

    public bool PasswordSet { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = [];
}
