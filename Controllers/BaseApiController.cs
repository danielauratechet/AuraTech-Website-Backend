using System.Diagnostics;
using AuratechBackend.Core.Responses;
using AuratechBackend.Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromResult(Result result, int? successStatusCode = null)
    {
        var traceId = GetTraceId();

        if (result.IsSuccess)
        {
            var statusCode = successStatusCode ?? result.StatusCode ?? StatusCodes.Status200OK;
            return StatusCode(statusCode, ApiResponse.Ok(message: result.Message, traceId: traceId));
        }

        return StatusCode(ResolveFailureStatusCode(result), ToFailureResponse(result, traceId));
    }

    protected IActionResult FromResult<T>(Result<T> result, int? successStatusCode = null)
    {
        var traceId = GetTraceId();

        if (result.IsSuccess)
        {
            var statusCode = successStatusCode ?? result.StatusCode ?? StatusCodes.Status200OK;
            return StatusCode(statusCode, ApiResponse<T>.Ok(result.Value, message: result.Message, traceId: traceId));
        }

        return StatusCode(ResolveFailureStatusCode(result), ToFailureResponse(result, traceId));
    }

    private static ApiResponse ToFailureResponse(Result result, string traceId)
    {
        var error = new ApiError
        {
            Code = result.Error.Code,
            Message = result.Error.Message,
            Details = result.ValidationErrors.Count == 0
                ? null
                : result.ValidationErrors
                    .Select(e => new ApiValidationError
                    {
                        PropertyName = e.PropertyName,
                        Message = e.Message
                    })
                    .ToList()
        };

        return ApiResponse.Fail(error, traceId: traceId);
    }

    private static int ResolveFailureStatusCode(Result result)
        => result.StatusCode ?? StatusCodes.Status400BadRequest;

    private string GetTraceId()
        => Activity.Current?.Id ?? HttpContext.TraceIdentifier;
}
