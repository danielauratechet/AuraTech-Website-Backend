using System.Diagnostics;
using AuratechBackend.Constants;
using AuratechBackend.Core.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuratechBackend.Filters;

public sealed class FluentValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (validationResult.IsValid)
            {
                continue;
            }

            var error = new ApiError
            {
                Code = ErrorCodes.Validation,
                Message = "One or more validation errors occurred.",
                Details = validationResult.Errors
                    .Select(e => new ApiValidationError
                    {
                        PropertyName = e.PropertyName,
                        Message = e.ErrorMessage
                    })
                    .ToList()
            };

            context.Result = new BadRequestObjectResult(
                ApiResponse.Fail(error, traceId: Activity.Current?.Id ?? context.HttpContext.TraceIdentifier));
            return;
        }

        await next();
    }
}
