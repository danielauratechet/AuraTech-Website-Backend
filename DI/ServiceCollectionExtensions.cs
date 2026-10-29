using AuratechBackend.Filters;
using AuratechBackend.Services.Audit;
using AuratechBackend.Services.Auth;
using AuratechBackend.Services.Campaigns;
using AuratechBackend.Services.CampaignTypes;
using AuratechBackend.Services.ConversionTracking;
using AuratechBackend.Services.Email;
using AuratechBackend.Services.Activity;
using AuratechBackend.Services.Kyc;
using AuratechBackend.Services.Organizations;
using AuratechBackend.Services.Regions;
using AuratechBackend.Services.Roles;
using AuratechBackend.Services.Storage;
using AuratechBackend.Services.Payments;
using AuratechBackend.Services.Users;
using AuratechBackend.Services.Wallet;
using FluentValidation;

namespace AuratechBackend.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        services.AddScoped<FluentValidationFilter>();

        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddHttpClient("Bird", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<IEmailSender, EmailService>();

        services.Configure<SupabaseOptions>(configuration.GetSection(SupabaseOptions.SectionName));
        services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordLinkService, PasswordLinkService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrganizationUserService, OrganizationUserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ICustomerKycService, CustomerKycService>();
        services.AddScoped<ICampaignTypeService, CampaignTypeService>();
        services.AddScoped<IRegionService, RegionService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ICampaignAnalyticsService, CampaignAnalyticsService>();
        services.AddScoped<IConversionTrackingService, ConversionTrackingService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IOrganizationProfileService, OrganizationProfileService>();
        services.AddScoped<IRecentActivityService, RecentActivityService>();

        services.Configure<StarpaySettings>(configuration.GetSection(StarpaySettings.SectionName));
        services.AddHttpClient<IStarpayClient, StarpayClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<IWalletService, WalletService>();

        return services;
    }
}
