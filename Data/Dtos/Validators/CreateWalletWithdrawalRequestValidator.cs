using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class CreateWalletWithdrawalRequestValidator : AbstractValidator<CreateWalletWithdrawalRequest>
{
    public CreateWalletWithdrawalRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true);

        RuleFor(x => x.Method)
            .IsInEnum();
    }
}
