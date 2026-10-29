using System.Text;
using AuratechBackend.Authorization;
using AuratechBackend.Constants;
using AuratechBackend.Core.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AuratechBackend.DI;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Keep claim types as issued (org_id, org_type, permission) instead of remapping.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "sub",
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");
                        logger.LogWarning(
                            context.Exception,
                            "JWT authentication failed: {Message}",
                            context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");
                        logger.LogDebug(
                            "JWT challenge: error={Error}, description={Description}",
                            context.Error,
                            context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });

        services.PostConfigure<AuthenticationOptions>(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, OrganizationTypeAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.OrgManage, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.OrgManage)));
            options.AddPolicy(AuthPolicies.UsersManage, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.UsersManage)));
            options.AddPolicy(AuthPolicies.UsersRead, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.UsersRead)));
            options.AddPolicy(AuthPolicies.RolesManage, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.RolesManage)));
            options.AddPolicy(AuthPolicies.KycRead, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.KycRead)));
            options.AddPolicy(AuthPolicies.KycWrite, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.KycWrite)));
            options.AddPolicy(AuthPolicies.KycUpdate, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.KycUpdate)));
            options.AddPolicy(AuthPolicies.KycPresign, policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.Claims.Any(c =>
                        c.Type == AppClaimTypes.Permission &&
                        (string.Equals(c.Value, PermissionCodes.KycWrite, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(c.Value, PermissionCodes.KycUpdate, StringComparison.OrdinalIgnoreCase)))));
            options.AddPolicy(AuthPolicies.KycView, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.KycView)));
            options.AddPolicy(AuthPolicies.KycApprove, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.KycApprove)));
            options.AddPolicy(AuthPolicies.KycReject, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.KycReject)));
            options.AddPolicy(AuthPolicies.CampaignTypesRead, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CampaignTypesRead)));
            options.AddPolicy(AuthPolicies.CampaignTypesManage, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CampaignTypesManage)));
            options.AddPolicy(AuthPolicies.RegionsRead, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.RegionsRead)));
            options.AddPolicy(AuthPolicies.RegionsManage, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.RegionsManage)));
            options.AddPolicy(AuthPolicies.CampaignsRead, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CampaignsRead)));
            options.AddPolicy(AuthPolicies.CampaignsWrite, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CampaignsWrite)));
            options.AddPolicy(AuthPolicies.CampaignsLaunch, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CampaignsLaunch)));
            options.AddPolicy(AuthPolicies.CampaignsPresign, policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.Claims.Any(c =>
                        c.Type == AppClaimTypes.Permission &&
                        string.Equals(c.Value, PermissionCodes.CampaignsWrite, StringComparison.OrdinalIgnoreCase))));
            options.AddPolicy(AuthPolicies.AuditLogsRead, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.AuditLogsRead)));
            options.AddPolicy(AuthPolicies.WalletRead, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.WalletRead)));
            options.AddPolicy(AuthPolicies.WalletWrite, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.WalletWrite)));
            options.AddPolicy(AuthPolicies.PlatformOrganization, policy =>
                policy.Requirements.Add(new OrganizationTypeRequirement(OrganizationType.Platform)));
            options.AddPolicy(AuthPolicies.CustomerOrganization, policy =>
                policy.Requirements.Add(new OrganizationTypeRequirement(OrganizationType.Customer)));
        });

        return services;
    }
}
