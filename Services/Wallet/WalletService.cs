using System.Globalization;
using System.Text.Json;
using AuratechBackend.Constants;
using AuratechBackend.Helpers;
using AuratechBackend.Core.Enums;
using AuratechBackend.Core.Responses;
using AuratechBackend.Core.Results;
using AuratechBackend.Data;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;
using AuratechBackend.Data.Entities;
using AuratechBackend.Services.Audit;
using AuratechBackend.Services.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuratechBackend.Services.Wallet;

public sealed class WalletService : IWalletService
{
    private const int RecentTransactionCount = 10;

    private readonly AppDbContext _db;
    private readonly IStarpayClient _starpay;
    private readonly IAuditLogService _auditLogService;
    private readonly IConfiguration _configuration;
    private readonly StarpaySettings _starpaySettings;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        AppDbContext db,
        IStarpayClient starpay,
        IAuditLogService auditLogService,
        IConfiguration configuration,
        IOptions<StarpaySettings> starpaySettings,
        ILogger<WalletService> logger)
    {
        _db = db;
        _starpay = starpay;
        _auditLogService = auditLogService;
        _configuration = configuration;
        _starpaySettings = starpaySettings.Value;
        _logger = logger;
    }

    public async Task<Result<OrganizationWalletResponse>> GetAsync(
        int organizationId,
        CancellationToken cancellationToken = default)
    {
        var wallet = await EnsureWalletAsync(organizationId, cancellationToken);
        var recent = await _db.WalletTransactions.AsNoTracking()
            .Where(t => t.OrganizationId == organizationId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ThenByDescending(t => t.Id)
            .Take(RecentTransactionCount)
            .ToListAsync(cancellationToken);

        var pending = await LatestWithdrawalAsync(organizationId, WalletTransactionStatus.Processing, cancellationToken);
        var lastCompleted = await LatestWithdrawalAsync(organizationId, WalletTransactionStatus.Completed, cancellationToken);

        return Result<OrganizationWalletResponse>.Success(new OrganizationWalletResponse
        {
            AvailableBalance = wallet.AvailableBalance,
            Currency = wallet.Currency,
            PendingWithdrawal = pending is null ? null : MapWithdrawal(pending),
            LastCompletedWithdrawal = lastCompleted is null ? null : MapWithdrawal(lastCompleted),
            RecentTransactions = recent.Select(MapTransaction).ToList()
        }, "Wallet retrieved successfully.");
    }

    public async Task<Result<PagedResult<WalletTransactionResponse>>> ListTransactionsAsync(
        int organizationId,
        WalletTransactionListQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var transactions = _db.WalletTransactions.AsNoTracking()
            .Where(t => t.OrganizationId == organizationId);

        var total = await transactions.CountAsync(cancellationToken);
        var items = await transactions
            .OrderByDescending(t => t.CreatedAtUtc)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result<PagedResult<WalletTransactionResponse>>.Success(new PagedResult<WalletTransactionResponse>
        {
            Items = items.Select(MapTransaction).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        }, "Transactions retrieved successfully.");
    }

    public async Task<Result<WalletTopUpResponse>> CreateTopUpAsync(
        int organizationId,
        int actorUserId,
        CreateWalletTopUpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsTopUpConfigured())
        {
            return Result<WalletTopUpResponse>.Failure(
                ErrorCodes.Unexpected,
                "Wallet top-up is not configured.",
                StatusCodes.Status500InternalServerError);
        }

        var userExists = await _db.Users.AsNoTracking()
            .AnyAsync(u => u.Id == actorUserId && u.OrganizationId == organizationId, cancellationToken);

        if (!userExists)
        {
            return Result<WalletTopUpResponse>.NotFound("User not found.");
        }

        if (!EthiopianPhoneNumbers.TryNormalize(request.CustomerPhoneNumber, out var phone))
        {
            return Result<WalletTopUpResponse>.ValidationFailure(
            [
                new ValidationError(nameof(request.CustomerPhoneNumber), "CustomerPhoneNumber must be an Ethiopian mobile number.")
            ]);
        }

        var customerName = request.CustomerName?.Trim();
        if (string.IsNullOrWhiteSpace(customerName))
        {
            customerName = "Customer";
        }

        var amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
        var now = DateTimeOffset.UtcNow;
        var wallet = await EnsureWalletAsync(organizationId, cancellationToken);
        var transaction = new WalletTransaction
        {
            WalletId = wallet.Id,
            OrganizationId = organizationId,
            Kind = WalletTransactionKind.TopUp,
            Direction = WalletTransactionDirection.Credit,
            Status = WalletTransactionStatus.Processing,
            Amount = amount,
            Currency = OrganizationWallet.CurrencyCode,
            Description = "Top-up via StarPay",
            Reference = $"pending-{Guid.NewGuid():N}",
            CreatedByUserId = actorUserId,
            CreatedAtUtc = now
        };

        _db.WalletTransactions.Add(transaction);
        await _db.SaveChangesAsync(cancellationToken);
        transaction.Reference = $"TX-{transaction.Id}";
        await _db.SaveChangesAsync(cancellationToken);

        var order = await _starpay.CreateOrderAsync(new StarpayCreateOrderCommand
        {
            Amount = amount,
            Description = "Wallet top-up",
            CustomerName = customerName,
            CustomerPhoneNumber = phone,
            CallbackUrl = _starpaySettings.WebhookUrl.Trim(),
            RedirectUrl = ResolveRedirectUrl(request.ReturnUrl)
        }, cancellationToken);

        if (!order.Succeeded || string.IsNullOrWhiteSpace(order.BillRefNo) || string.IsNullOrWhiteSpace(order.PaymentUrl))
        {
            transaction.Status = WalletTransactionStatus.Failed;
            transaction.CompletedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return Result<WalletTopUpResponse>.Failure(
                ErrorCodes.Failure,
                order.ErrorMessage ?? "StarPay could not create the payment.",
                StatusCodes.Status502BadGateway);
        }

        transaction.StarpayBillRefNo = order.BillRefNo;
        transaction.PaymentUrl = order.PaymentUrl;
        await _db.SaveChangesAsync(cancellationToken);

        return await SettleTopUpAsync(
            transaction.Id,
            webhookStatus: null,
            paymentChannel: null,
            returnProcessingWhenVerifyFails: true,
            cancellationToken);
    }

    public Task<Result<WalletTopUpResponse>> SyncTopUpAsync(
        int organizationId,
        int transactionId,
        CancellationToken cancellationToken = default)
        => SettleTopUpAsync(
            transactionId,
            webhookStatus: null,
            paymentChannel: null,
            returnProcessingWhenVerifyFails: false,
            cancellationToken,
            organizationId);

    public async Task<Result<WalletTransactionResponse>> CreateWithdrawalAsync(
        int organizationId,
        int actorUserId,
        CreateWalletWithdrawalRequest request,
        CancellationToken cancellationToken = default)
    {
        var amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
        var wallet = await EnsureWalletAsync(organizationId, cancellationToken);

        await using var dbTransaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        await LockWalletAsync(wallet.Id, cancellationToken);
        await _db.Entry(wallet).ReloadAsync(cancellationToken);

        if (wallet.AvailableBalance < amount)
        {
            return Result<WalletTransactionResponse>.Conflict(
                $"Available balance is {FormatMoney(wallet.AvailableBalance)} ETB. Withdrawal amount is {FormatMoney(amount)} ETB.");
        }

        var now = DateTimeOffset.UtcNow;
        wallet.AvailableBalance -= amount;
        wallet.UpdatedAtUtc = now;

        var transaction = new WalletTransaction
        {
            WalletId = wallet.Id,
            OrganizationId = organizationId,
            Kind = WalletTransactionKind.Withdrawal,
            Direction = WalletTransactionDirection.Debit,
            Status = WalletTransactionStatus.Completed,
            Amount = amount,
            BalanceAfter = wallet.AvailableBalance,
            Currency = OrganizationWallet.CurrencyCode,
            Description = "Withdrawal to Bank Account",
            Reference = $"pending-{Guid.NewGuid():N}",
            Method = request.Method.ToDisplayName(),
            CreatedByUserId = actorUserId,
            CreatedAtUtc = now,
            CompletedAtUtc = now
        };

        _db.WalletTransactions.Add(transaction);
        await _db.SaveChangesAsync(cancellationToken);
        transaction.Reference = $"WD-{transaction.Id}";
        await _db.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);

        await WriteBillingAuditAsync(
            organizationId,
            actorUserId,
            AuditActionCodes.BillingWithdrawn,
            $"withdrew {FormatMoney(amount)} ETB from the wallet.",
            transaction.Reference,
            cancellationToken);

        return Result<WalletTransactionResponse>.Success(
            MapTransaction(transaction),
            "Withdrawal completed successfully.");
    }

    public async Task<WebhookHandlingResult> HandleWebhookAsync(
        string rawBody,
        string? timestamp,
        string? signature,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(signature))
        {
            return new WebhookHandlingResult(StatusCodes.Status400BadRequest, "Missing signature headers.");
        }

        if (!StarpaySignature.IsValid(rawBody, timestamp, signature, _starpaySettings.WebhookSecretKey))
        {
            return new WebhookHandlingResult(StatusCodes.Status401Unauthorized, "Invalid signature.");
        }

        if (!StarpaySignature.IsFresh(timestamp, DateTimeOffset.UtcNow))
        {
            return new WebhookHandlingResult(StatusCodes.Status400BadRequest, "Stale callback timestamp.");
        }

        if (!TryParseCallback(rawBody, out var callback))
        {
            return new WebhookHandlingResult(StatusCodes.Status400BadRequest, "Invalid callback payload.");
        }

        if (string.IsNullOrWhiteSpace(callback.BillRefNo))
        {
            return new WebhookHandlingResult(StatusCodes.Status400BadRequest, "Callback is missing a bill reference.");
        }

        var transaction = await _db.WalletTransactions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.StarpayBillRefNo == callback.BillRefNo, cancellationToken);

        if (transaction is null)
        {
            _logger.LogWarning("StarPay callback for unknown bill {BillRefNo}", callback.BillRefNo);
            return new WebhookHandlingResult(StatusCodes.Status200OK, "Callback accepted.");
        }

        var settled = await SettleTopUpAsync(
            transaction.Id,
            callback.Status,
            callback.PaymentChannel,
            returnProcessingWhenVerifyFails: false,
            cancellationToken,
            transaction.OrganizationId);

        if (settled.IsFailure && settled.StatusCode == StatusCodes.Status502BadGateway)
        {
            return new WebhookHandlingResult(StatusCodes.Status502BadGateway, "Payment could not be verified.");
        }

        return new WebhookHandlingResult(StatusCodes.Status200OK, "Callback processed.");
    }

    private async Task<Result<WalletTopUpResponse>> SettleTopUpAsync(
        int transactionId,
        string? webhookStatus,
        string? paymentChannel,
        bool returnProcessingWhenVerifyFails,
        CancellationToken cancellationToken,
        int? organizationId = null)
    {
        var existing = await _db.WalletTransactions.AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Id == transactionId && (organizationId == null || t.OrganizationId == organizationId),
                cancellationToken);

        if (existing is null || existing.Kind != WalletTransactionKind.TopUp)
        {
            return Result<WalletTopUpResponse>.NotFound("Top-up not found.");
        }

        if (existing.Status != WalletTransactionStatus.Processing)
        {
            return Result<WalletTopUpResponse>.Success(await MapTopUpAsync(existing, cancellationToken), "Top-up retrieved successfully.");
        }

        if (string.IsNullOrWhiteSpace(existing.StarpayBillRefNo))
        {
            return Result<WalletTopUpResponse>.Conflict("Payment has not been started with StarPay.");
        }

        var verification = await _starpay.VerifyAsync(existing.StarpayBillRefNo, cancellationToken);
        if (verification.CallFailed)
        {
            if (returnProcessingWhenVerifyFails && !string.IsNullOrWhiteSpace(existing.PaymentUrl))
            {
                return Result<WalletTopUpResponse>.Success(
                    await MapTopUpAsync(existing, cancellationToken),
                    "Top-up started. Payment is still processing.");
            }

            return Result<WalletTopUpResponse>.Failure(
                ErrorCodes.Failure,
                "StarPay could not verify the payment.",
                StatusCodes.Status502BadGateway);
        }

        await using var dbTransaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        await LockTransactionAsync(existing.Id, cancellationToken);

        var transaction = await _db.WalletTransactions.FirstAsync(t => t.Id == existing.Id, cancellationToken);
        await _db.Entry(transaction).ReloadAsync(cancellationToken);

        if (transaction.Status != WalletTransactionStatus.Processing)
        {
            await dbTransaction.CommitAsync(cancellationToken);
            return Result<WalletTopUpResponse>.Success(
                await MapTopUpAsync(transaction, cancellationToken),
                "Top-up retrieved successfully.");
        }

        if (verification.IsPaid)
        {
            var verifiedAmount = verification.Amount.HasValue
                ? decimal.Round(verification.Amount.Value, 2, MidpointRounding.AwayFromZero)
                : (decimal?)null;

            if (verifiedAmount != transaction.Amount)
            {
                _logger.LogWarning(
                    "StarPay amount {VerifiedAmount} did not match top-up {TransactionId} amount {Amount}",
                    verifiedAmount,
                    transaction.Id,
                    transaction.Amount);
                return Result<WalletTopUpResponse>.Conflict(
                    "StarPay reported an amount that does not match this top-up.");
            }

            await LockWalletAsync(transaction.WalletId, cancellationToken);
            var wallet = await _db.OrganizationWallets.FirstAsync(w => w.Id == transaction.WalletId, cancellationToken);
            await _db.Entry(wallet).ReloadAsync(cancellationToken);

            var now = DateTimeOffset.UtcNow;
            wallet.AvailableBalance += transaction.Amount;
            wallet.UpdatedAtUtc = now;
            transaction.Status = WalletTransactionStatus.Completed;
            transaction.BalanceAfter = wallet.AvailableBalance;
            transaction.CompletedAtUtc = now;
            if (!string.IsNullOrWhiteSpace(paymentChannel))
            {
                transaction.Method = paymentChannel.Trim();
            }

            transaction.Description = string.IsNullOrWhiteSpace(transaction.Method)
                ? "Top-up via StarPay"
                : $"Top-up via {transaction.Method}";

            await _db.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            await WriteBillingAuditAsync(
                transaction.OrganizationId,
                transaction.CreatedByUserId,
                AuditActionCodes.BillingDeposited,
                $"deposited {FormatMoney(transaction.Amount)} ETB to the wallet.",
                transaction.Reference,
                cancellationToken);

            return Result<WalletTopUpResponse>.Success(
                await MapTopUpAsync(transaction, cancellationToken),
                "Top-up completed successfully.");
        }

        if (verification.IsFailed || StarpayVerifyResult.IsFailedStatus(webhookStatus))
        {
            transaction.Status = WalletTransactionStatus.Failed;
            transaction.CompletedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);
            return Result<WalletTopUpResponse>.Success(
                await MapTopUpAsync(transaction, cancellationToken),
                "Top-up failed.");
        }

        await dbTransaction.CommitAsync(cancellationToken);
        return Result<WalletTopUpResponse>.Success(
            await MapTopUpAsync(transaction, cancellationToken),
            "Top-up started. Payment is still processing.");
    }

    private async Task<OrganizationWallet> EnsureWalletAsync(int organizationId, CancellationToken cancellationToken)
    {
        var wallet = await _db.OrganizationWallets
            .FirstOrDefaultAsync(w => w.OrganizationId == organizationId, cancellationToken);

        if (wallet is not null)
        {
            return wallet;
        }

        wallet = new OrganizationWallet
        {
            OrganizationId = organizationId,
            AvailableBalance = 0,
            Currency = OrganizationWallet.CurrencyCode,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _db.OrganizationWallets.Add(wallet);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return wallet;
        }
        catch (DbUpdateException)
        {
            _db.Entry(wallet).State = EntityState.Detached;
            return await _db.OrganizationWallets.FirstAsync(w => w.OrganizationId == organizationId, cancellationToken);
        }
    }

    private Task LockWalletAsync(int walletId, CancellationToken cancellationToken)
        => _db.Database.ExecuteSqlInterpolatedAsync(
            $"""SELECT 1 FROM "OrganizationWallets" WHERE "Id" = {walletId} FOR UPDATE""",
            cancellationToken);

    private Task LockTransactionAsync(int transactionId, CancellationToken cancellationToken)
        => _db.Database.ExecuteSqlInterpolatedAsync(
            $"""SELECT 1 FROM "WalletTransactions" WHERE "Id" = {transactionId} FOR UPDATE""",
            cancellationToken);

    private async Task<WalletTransaction?> LatestWithdrawalAsync(
        int organizationId,
        WalletTransactionStatus status,
        CancellationToken cancellationToken)
        => await _db.WalletTransactions.AsNoTracking()
            .Where(t => t.OrganizationId == organizationId
                        && t.Kind == WalletTransactionKind.Withdrawal
                        && t.Status == status)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ThenByDescending(t => t.Id)
            .FirstOrDefaultAsync(cancellationToken);

    private async Task<WalletTopUpResponse> MapTopUpAsync(WalletTransaction transaction, CancellationToken cancellationToken)
    {
        var balance = transaction.BalanceAfter
                      ?? await _db.OrganizationWallets.AsNoTracking()
                          .Where(w => w.Id == transaction.WalletId)
                          .Select(w => w.AvailableBalance)
                          .FirstAsync(cancellationToken);

        return new WalletTopUpResponse
        {
            TransactionId = transaction.Id,
            Reference = transaction.Reference,
            BillRefNo = transaction.StarpayBillRefNo,
            PaymentUrl = transaction.PaymentUrl,
            Status = transaction.Status,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            AvailableBalance = balance
        };
    }

    private static WalletTransactionResponse MapTransaction(WalletTransaction transaction)
        => new()
        {
            Id = transaction.Id,
            Reference = transaction.Reference,
            Description = transaction.Description,
            Direction = transaction.Direction,
            Kind = transaction.Kind,
            Status = transaction.Status,
            Method = transaction.Method,
            Amount = transaction.Amount,
            SignedAmount = transaction.Direction == WalletTransactionDirection.Credit
                ? transaction.Amount
                : -transaction.Amount,
            BalanceAfter = transaction.BalanceAfter,
            Currency = transaction.Currency,
            CreatedAtUtc = transaction.CreatedAtUtc,
            CompletedAtUtc = transaction.CompletedAtUtc
        };

    private static WalletWithdrawalResponse MapWithdrawal(WalletTransaction transaction)
        => new()
        {
            Id = transaction.Id,
            Reference = transaction.Reference,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Status = transaction.Status,
            Method = transaction.Method,
            CreatedAtUtc = transaction.CreatedAtUtc,
            CompletedAtUtc = transaction.CompletedAtUtc
        };

    private async Task WriteBillingAuditAsync(
        int organizationId,
        int actorUserId,
        string actionCode,
        string summary,
        string reference,
        CancellationToken cancellationToken)
    {
        var actor = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);

        await _auditLogService.WriteAsync(new AuditLogWriteRequest
        {
            OrganizationId = organizationId,
            ActorUserId = actorUserId,
            ActorDisplayName = AuditActorNames.Format(actor),
            ActionCode = actionCode,
            ActionSummary = summary,
            HighlightedTarget = reference,
            Category = AuditLogCategory.Billing,
            Status = AuditLogStatus.Success,
            ResourceType = "WalletTransaction",
            ResourceId = reference,
            LinkPath = "/dashboard/wallet",
            LinkText = "View Wallet"
        }, cancellationToken);
    }

    private bool IsTopUpConfigured()
        => !string.IsNullOrWhiteSpace(_starpaySettings.BaseUrl)
           && !string.IsNullOrWhiteSpace(_starpaySettings.SecretKey)
           && !string.IsNullOrWhiteSpace(_starpaySettings.WebhookUrl);

    private string? ResolveRedirectUrl(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            return returnUrl.Trim();
        }

        var publicBaseUrl = _configuration["App:PublicBaseUrl"]?.Trim();
        return string.IsNullOrWhiteSpace(publicBaseUrl) ? null : publicBaseUrl;
    }

    private static string FormatMoney(decimal amount)
        => amount.ToString("0.00", CultureInfo.InvariantCulture);

    private static bool TryParseCallback(string rawBody, out StarpayCallback callback)
    {
        callback = default;
        try
        {
            using var document = JsonDocument.Parse(rawBody);
            var root = document.RootElement;
            callback = new StarpayCallback(
                ReadString(root, "billRefNo", "bill_ref_no"),
                ReadString(root, "status"),
                ReadString(root, "paymentChannel", "payment_type", "paymentType"));
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string? ReadString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }

        return null;
    }

    private readonly record struct StarpayCallback(string? BillRefNo, string? Status, string? PaymentChannel);
}
