using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AuratechBackend.DI;

public static class ObservabilityServiceCollectionExtensions
{
    public const string ServiceName = "AuratechBackend";

    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = httpContext => !ShouldIgnorePath(httpContext.Request.Path);
                    })
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                }
            });

        return services;
    }

    private static bool ShouldIgnorePath(PathString path)
        => path.StartsWithSegments("/swagger")
           || path.StartsWithSegments("/favicon.ico");
}
