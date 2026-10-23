using AuratechBackend.Data.Seed;
using AuratechBackend.DI;
using AuratechBackend.Filters;
using AuratechBackend.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .WriteTo.Console());

    builder.Services.AddObservability(builder.Configuration, builder.Environment);
    builder.Services.AddData(builder.Configuration);
    builder.Services.AddAuth(builder.Configuration);
    builder.Services.AddServices(builder.Configuration);

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<FluentValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "AuratechBackend API",
            Version = "v1",
            Description = "Google Ads platform API"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Paste only the accessToken value from POST /api/v1/auth/login " +
                "(do not include the 'Bearer ' prefix). Click Authorize, then Close."
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        options.DocumentFilter<SwaggerBearerDocumentFilter>();
        options.OperationFilter<SwaggerBearerOperationFilter>();
    });

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = (httpContext, _, exception) =>
        {
            if (exception is not null)
            {
                return LogEventLevel.Error;
            }

            var path = httpContext.Request.Path;
            if (path.StartsWithSegments("/swagger") || path.StartsWithSegments("/favicon.ico"))
            {
                return LogEventLevel.Verbose;
            }

            return LogEventLevel.Information;
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "AuratechBackend API v1");
        });
    }

    app.UseCors();

    // Skip HTTPS redirection in Development so Swagger on HTTP does not drop Authorization.
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await IdentitySeed.MigrateAsync(app.Services);

    var seedEnabled = app.Configuration.GetValue("Seed:Enabled", false);
    if (app.Environment.IsDevelopment() || seedEnabled)
    {
        await IdentitySeed.SeedAsync(app.Services);
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
