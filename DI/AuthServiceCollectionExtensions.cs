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
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.FromMinutes(1)
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
            options.AddPolicy(AuthPolicies.KycView, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.KycView)));
            options.AddPolicy(AuthPolicies.KycApprove, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.KycApprove)));
            options.AddPolicy(AuthPolicies.KycReject, policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionCodes.KycReject)));
            options.AddPolicy(AuthPolicies.PlatformOrganization, policy =>
                policy.Requirements.Add(new OrganizationTypeRequirement(OrganizationType.Platform)));
            options.AddPolicy(AuthPolicies.CustomerOrganization, policy =>
                policy.Requirements.Add(new OrganizationTypeRequirement(OrganizationType.Customer)));
        });

        return services;
    }
}
