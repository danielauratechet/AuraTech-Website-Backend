using System.Diagnostics;
using AuratechBackend.Helpers;

namespace AuratechBackend.Middleware;

public sealed class RequestResponseLoggingMiddleware
{
    private static readonly PathString[] SkippedPrefixes =
    [
        new("/swagger"),
        new("/favicon.ico")
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly int _maxBodyLength;
    private readonly bool _logBodies;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _maxBodyLength = configuration.GetValue("Observability:MaxBodyLength", 32 * 1024);
        _logBodies = configuration.GetValue("Observability:LogRequestResponseBodies", true);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestBody = string.Empty;

        if (_logBodies)
        {
            context.Request.EnableBuffering();
            requestBody = await HttpBodyReader.ReadAsStringAsync(
                context.Request.Body,
                _maxBodyLength,
                context.RequestAborted);
            requestBody = SensitiveDataRedactor.Redact(requestBody);
        }

        var originalBodyStream = context.Response.Body;
        await using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var responseBody = string.Empty;
            if (_logBodies)
            {
                responseBodyStream.Position = 0;
                responseBody = await HttpBodyReader.ReadAsStringAsync(
                    responseBodyStream,
                    _maxBodyLength,
                    context.RequestAborted);
                responseBody = SensitiveDataRedactor.Redact(responseBody);

                responseBodyStream.Position = 0;
                await responseBodyStream.CopyToAsync(originalBodyStream, context.RequestAborted);
            }
            else
            {
                responseBodyStream.Position = 0;
                await responseBodyStream.CopyToAsync(originalBodyStream, context.RequestAborted);
            }

            context.Response.Body = originalBodyStream;

            var activity = Activity.Current;
            _logger.LogInformation(
                "HTTP {Method} {Path}{Query} responded {StatusCode} in {ElapsedMs}ms TraceId={TraceId} SpanId={SpanId} RequestBody={RequestBody} ResponseBody={ResponseBody}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                activity?.TraceId.ToString() ?? context.TraceIdentifier,
                activity?.SpanId.ToString() ?? string.Empty,
                requestBody,
                responseBody);
        }
    }

    private static bool ShouldSkip(PathString path)
        => SkippedPrefixes.Any(prefix => path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));
}
