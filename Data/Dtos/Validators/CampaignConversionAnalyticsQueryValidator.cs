using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class CampaignConversionAnalyticsQueryValidator : AbstractValidator<CampaignConversionAnalyticsQuery>
{
    public CampaignConversionAnalyticsQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From <= q.To)
            .WithMessage("From must be on or before To.")
            .WithName(nameof(CampaignConversionAnalyticsQuery.From));

        RuleFor(x => x)
            .Must(q =>
            {
                if (!q.From.HasValue || !q.To.HasValue)
                {
                    return true;
                }

                return q.To.Value.DayNumber - q.From.Value.DayNumber <= 366;
            })
            .WithMessage("Date range cannot exceed 366 days.")
            .WithName(nameof(CampaignConversionAnalyticsQuery.To));
    }
}
