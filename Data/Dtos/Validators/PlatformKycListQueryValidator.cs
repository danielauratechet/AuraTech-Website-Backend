using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class PlatformKycListQueryValidator : AbstractValidator<PlatformKycListQuery>
{
    public PlatformKycListQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("'From' must be less than or equal to 'To'.");
    }
}
