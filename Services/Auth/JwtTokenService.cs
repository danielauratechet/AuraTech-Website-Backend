using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuratechBackend.Constants;
using AuratechBackend.Data;
using AuratechBackend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuratechBackend.Services.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public JwtTokenService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<(string Token, DateTimeOffset ExpiresAtUtc)> CreateTokenAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        var organization = await _db.Organizations
            .AsNoTracking()
            .SingleAsync(o => o.Id == user.OrganizationId, cancellationToken);

        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        var roles = await _db.Roles
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name!)
            .ToListAsync(cancellationToken);

        var permissions = await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var expiryMinutes = _configuration.GetValue("Jwt:ExpiryMinutes", 60);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(AppClaimTypes.OrganizationId, user.OrganizationId.ToString()),
            new(AppClaimTypes.OrganizationType, organization.Type.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(permission => new Claim(AppClaimTypes.Permission, permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
