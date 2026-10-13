using System.Text.RegularExpressions;
using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed partial class CreateRegionRequestValidator : AbstractValidator<CreateRegionRequest>
{
    public CreateRegionRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(8)
            .Must(code => RegionCodeRegex().IsMatch(code))
            .WithMessage("Code must be 2-8 uppercase letters.");

        RuleFor(x => x.NameEn)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.NameAm)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.EstimatedUsers)
            .GreaterThanOrEqualTo(0);
    }

    [GeneratedRegex("^[A-Z]{2,8}$")]
    private static partial Regex RegionCodeRegex();
}
