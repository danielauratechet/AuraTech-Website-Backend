using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/audit-logs")]
public sealed class CustomerAuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public CustomerAuditLogsController(IAuditLogService auditLogService)
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
