using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class UpsertConversionTrackingRequestValidator : AbstractValidator<UpsertConversionTrackingRequest>
{
    public static readonly int[] AllowedClickThroughWindows = [1, 7, 30, 90];

    public UpsertConversionTrackingRequestValidator()
    {
        RuleFor(x => x.ConversionType)
            .IsInEnum();

        RuleFor(x => x.GoalName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CountMode)
            .IsInEnum();

        RuleFor(x => x.ConversionValueEtb)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ClickThroughWindowDays)
            .Must(days => AllowedClickThroughWindows.Contains(days))
            .WithMessage("ClickThroughWindowDays must be one of: 1, 7, 30, 90.");

        RuleFor(x => x.InstallMethod)
            .IsInEnum();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
