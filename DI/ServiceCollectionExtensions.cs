using AuratechBackend.Filters;
using FluentValidation;

namespace AuratechBackend.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        services.AddScoped<FluentValidationFilter>();

        return services;
    }
}
