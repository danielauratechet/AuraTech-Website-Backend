using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Platform;

[Authorize(Policy = AuthPolicies.PlatformOrganization)]
[Route(ApiRoutes.Prefix + "/platform/audit-logs")]
public sealed class PlatformAuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public PlatformAuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.AuditLogsRead)]
    public async Task<IActionResult> List(
        [FromQuery] AuditLogListQuery query,
        CancellationToken cancellationToken)
        => FromResult(await _auditLogService.ListAsync(
            User.GetOrganizationId(),
            query,
            cancellationToken));

    [HttpGet("summary")]
    [Authorize(Policy = AuthPolicies.AuditLogsRead)]
    public async Task<IActionResult> Summary(
        [FromQuery] AuditLogSummaryQuery query,
        CancellationToken cancellationToken)
        => FromResult(await _auditLogService.GetSummaryAsync(
            User.GetOrganizationId(),
            query,
            cancellationToken));
}
