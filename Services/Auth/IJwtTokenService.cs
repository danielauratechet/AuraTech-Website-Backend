using AuratechBackend.Data.Entities;

namespace AuratechBackend.Services.Auth;

public interface IJwtTokenService
{
    Task<(string Token, DateTimeOffset ExpiresAtUtc)> CreateTokenAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default);
}
