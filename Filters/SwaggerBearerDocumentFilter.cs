using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuratechBackend.Filters;

/// <summary>
/// Applies global Bearer security so Swagger UI sends Authorization on requests.
/// </summary>
public sealed class SwaggerBearerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Security ??= [];
        swaggerDoc.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", swaggerDoc)] = []
        });
    }
}
