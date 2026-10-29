using System.Text;
using AuratechBackend.Constants;
using AuratechBackend.Services.Wallet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers;

[AllowAnonymous]
[ApiController]
[Route(ApiRoutes.Prefix + "/webhooks/starpay")]
public sealed class StarpayWebhookController : ControllerBase
{
    private const int MaxBodyLength = 1_000_000;

    private readonly IWalletService _walletService;

    public StarpayWebhookController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(
            Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        if (rawBody.Length > MaxBodyLength)
        {
            return BadRequest(new { message = "Callback payload is too large." });
        }

        var result = await _walletService.HandleWebhookAsync(
            rawBody,
            Request.Headers["X-Timestamp"].ToString(),
            Request.Headers["X-Signature"].ToString(),
            cancellationToken);

        return StatusCode(result.StatusCode, new { message = result.Message });
    }
}
