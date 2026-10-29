using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Wallet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers.Customer;

[Authorize(Policy = AuthPolicies.CustomerOrganization)]
[Route(ApiRoutes.Prefix + "/customer/wallet")]
public sealed class CustomerWalletController : BaseApiController
{
    private readonly IWalletService _walletService;

    public CustomerWalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.WalletRead)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => FromResult(await _walletService.GetAsync(User.GetOrganizationId(), cancellationToken));

    [HttpGet("transactions")]
    [Authorize(Policy = AuthPolicies.WalletRead)]
    public async Task<IActionResult> Transactions(
        [FromQuery] WalletTransactionListQuery query,
        CancellationToken cancellationToken)
        => FromResult(await _walletService.ListTransactionsAsync(
            User.GetOrganizationId(),
            query,
            cancellationToken));

    [HttpPost("top-ups")]
    [Authorize(Policy = AuthPolicies.WalletWrite)]
    public async Task<IActionResult> CreateTopUp(
        [FromBody] CreateWalletTopUpRequest request,
        CancellationToken cancellationToken)
        => FromResult(
            await _walletService.CreateTopUpAsync(
                User.GetOrganizationId(),
                User.GetUserId(),
                request,
                cancellationToken),
            StatusCodes.Status201Created);

    [HttpPost("top-ups/{id:int}/sync")]
    [Authorize(Policy = AuthPolicies.WalletWrite)]
    public async Task<IActionResult> SyncTopUp(int id, CancellationToken cancellationToken)
        => FromResult(await _walletService.SyncTopUpAsync(
            User.GetOrganizationId(),
            id,
            cancellationToken));

    [HttpPost("withdrawals")]
    [Authorize(Policy = AuthPolicies.WalletWrite)]
    public async Task<IActionResult> CreateWithdrawal(
        [FromBody] CreateWalletWithdrawalRequest request,
        CancellationToken cancellationToken)
        => FromResult(
            await _walletService.CreateWithdrawalAsync(
                User.GetOrganizationId(),
                User.GetUserId(),
                request,
                cancellationToken),
            StatusCodes.Status201Created);
}
