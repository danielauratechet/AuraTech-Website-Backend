using System.Diagnostics;
using AuratechBackend.Constants;
using AuratechBackend.Core.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        if (!context.ModelState.IsValid)
        {
            context.Result = ValidationBadRequest(
                context,
                context.ModelState
                    .Where(entry => entry.Value is { Errors.Count: > 0 })
                    .SelectMany(entry => entry.Value!.Errors.Select(error => new ApiValidationError
                    {
                        PropertyName = NormalizeModelStateKey(entry.Key),
                        Message = string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "The value is invalid."
                            : error.ErrorMessage
                    }))
                    .ToList());
            return;
        }

        if (HasMissingRequiredBody(context, out var bodyParameterName))
        {
            context.Result = ValidationBadRequest(
                context,
                [
                    new ApiValidationError
                    {
                        PropertyName = bodyParameterName ?? "body",
                        Message = "Request body is required or could not be parsed."
                    }
                ]);
            return;
        }

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

            context.Result = ValidationBadRequest(
                context,
                validationResult.Errors
                    .Select(e => new ApiValidationError
                    {
                        PropertyName = e.PropertyName,
                        Message = e.ErrorMessage
                    })
                    .ToList());
            return;
        }

        await next();
    }

    private static bool HasMissingRequiredBody(ActionExecutingContext context, out string? parameterName)
    {
        parameterName = null;

        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            return false;
        }

        foreach (var parameter in descriptor.Parameters)
        {
            if (parameter.BindingInfo?.BindingSource != BindingSource.Body)
            {
                continue;
            }

            parameterName = parameter.Name;

            if (!context.ActionArguments.TryGetValue(parameter.Name!, out var value) || value is null)
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeModelStateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key == "$")
        {
            return "body";
        }

        return key.StartsWith("$.") ? key[2..] : key;
    }

    private static BadRequestObjectResult ValidationBadRequest(
        ActionExecutingContext context,
        IReadOnlyList<ApiValidationError> details)
    {
        var error = new ApiError
        {
            Code = ErrorCodes.Validation,
            Message = "One or more validation errors occurred.",
            Details = details
        };

        return new BadRequestObjectResult(
            ApiResponse.Fail(error, traceId: Activity.Current?.Id ?? context.HttpContext.TraceIdentifier));
    }
}
