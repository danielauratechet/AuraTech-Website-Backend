using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;

namespace AuratechBackend.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static int GetOrganizationId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(AppClaimTypes.OrganizationId);
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static OrganizationType? GetOrganizationType(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(AppClaimTypes.OrganizationType);
        return Enum.TryParse<OrganizationType>(value, out var type) ? type : null;
    }
}
