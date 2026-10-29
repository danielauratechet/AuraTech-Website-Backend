using AuratechBackend.Core.Responses;
using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Wallet;

public interface IWalletService
{
    Task<Result<OrganizationWalletResponse>> GetAsync(
        int organizationId,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<WalletTransactionResponse>>> ListTransactionsAsync(
        int organizationId,
        WalletTransactionListQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<WalletTopUpResponse>> CreateTopUpAsync(
        int organizationId,
        int actorUserId,
        CreateWalletTopUpRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<WalletTopUpResponse>> SyncTopUpAsync(
        int organizationId,
        int transactionId,
        CancellationToken cancellationToken = default);

    Task<Result<WalletTransactionResponse>> CreateWithdrawalAsync(
        int organizationId,
        int actorUserId,
        CreateWalletWithdrawalRequest request,
        CancellationToken cancellationToken = default);

    Task<WebhookHandlingResult> HandleWebhookAsync(
        string rawBody,
        string? timestamp,
        string? signature,
        CancellationToken cancellationToken = default);
}

public readonly record struct WebhookHandlingResult(int StatusCode, string Message);
