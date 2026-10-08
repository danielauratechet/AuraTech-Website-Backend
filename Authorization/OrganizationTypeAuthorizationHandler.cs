using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AuratechBackend.Authorization;

public sealed class OrganizationTypeRequirement : IAuthorizationRequirement
{
    public OrganizationTypeRequirement(OrganizationType organizationType)
    {
        OrganizationType = organizationType;
    }

    public OrganizationType OrganizationType { get; }
}

public sealed class OrganizationTypeAuthorizationHandler : AuthorizationHandler<OrganizationTypeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganizationTypeRequirement requirement)
    {
        var value = context.User.FindFirst(AppClaimTypes.OrganizationType)?.Value;
        if (Enum.TryParse<OrganizationType>(value, out var type) && type == requirement.OrganizationType)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
