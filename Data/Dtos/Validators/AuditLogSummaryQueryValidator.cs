using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class AuditLogSummaryQueryValidator : AbstractValidator<AuditLogSummaryQuery>
{
    public AuditLogSummaryQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("'From' must be less than or equal to 'To'.")
            .WithName(nameof(AuditLogSummaryQuery.From));

        RuleFor(x => x)
            .Must(x =>
            {
                if (!x.From.HasValue || !x.To.HasValue)
                {
                    return true;
                }

                return x.To.Value.DayNumber - x.From.Value.DayNumber <= 366;
            })
            .WithMessage("Date range cannot exceed 366 days.")
            .WithName(nameof(AuditLogSummaryQuery.To));
    }
}
