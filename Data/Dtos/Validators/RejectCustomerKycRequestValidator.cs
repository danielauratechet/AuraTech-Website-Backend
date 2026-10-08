using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class RejectCustomerKycRequestValidator : AbstractValidator<RejectCustomerKycRequest>
{
    public RejectCustomerKycRequestValidator()
    {
        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
