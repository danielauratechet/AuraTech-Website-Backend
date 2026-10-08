using AuratechBackend.Filters;
using AuratechBackend.Services.Auth;
using AuratechBackend.Services.Email;
using AuratechBackend.Services.Kyc;
using AuratechBackend.Services.Roles;
using AuratechBackend.Services.Users;
using FluentValidation;

namespace AuratechBackend.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        services.AddScoped<FluentValidationFilter>();

        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailSender, EmailService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordLinkService, PasswordLinkService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrganizationUserService, OrganizationUserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ICustomerKycService, CustomerKycService>();

        return services;
    }
}
