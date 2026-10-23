using AuratechBackend.Core.Enums;
using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class SaveCampaignStep3RequestValidator : AbstractValidator<SaveCampaignStep3Request>
{
    public SaveCampaignStep3RequestValidator()
    {
        RuleFor(x => x.DailyBudget)
            .GreaterThan(0);

        RuleFor(x => x.ScheduleType)
            .IsInEnum();

        RuleFor(x => x.EndDate)
            .NotNull()
            .When(x => x.ScheduleType == CampaignScheduleType.EndDate)
            .WithMessage("EndDate is required when schedule type is EndDate.");

        RuleFor(x => x.EndDate)
            .Must(d => d is null || d >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .When(x => x.ScheduleType == CampaignScheduleType.EndDate)
            .WithMessage("EndDate must be today or a future date.");

        RuleFor(x => x.Headline)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(90);

        RuleFor(x => x.CallToAction)
            .IsInEnum();

        RuleFor(x => x.CreativeImageUrl)
            .MaximumLength(2048)
            .Must(BeAbsoluteHttpsUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.CreativeImageUrl))
            .WithMessage("CreativeImageUrl must be a valid absolute HTTPS URL.");
    }

    private static bool BeAbsoluteHttpsUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && uri.Scheme == Uri.UriSchemeHttps;
}
