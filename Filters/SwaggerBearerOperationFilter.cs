using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuratechBackend.Filters;

/// <summary>
/// Clears security on anonymous endpoints so global Bearer does not apply to them.
/// </summary>
public sealed class SwaggerBearerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!HasAllowAnonymous(context))
        {
            return;
        }

        // Empty security array = no auth required for this operation (overrides document-level Bearer).
        operation.Security = [];
    }

    private static bool HasAllowAnonymous(OperationFilterContext context)
    {
        if (context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any())
        {
            return true;
        }

        if (context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any() == true)
        {
            return true;
        }

        if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            return descriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        }

        return false;
    }
}
