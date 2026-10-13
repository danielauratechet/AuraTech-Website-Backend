using System.Text.RegularExpressions;
using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed partial class CreateCampaignTypeRequestValidator : AbstractValidator<CreateCampaignTypeRequest>
{
    public CreateCampaignTypeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(64)
            .Must(code => CampaignTypeCodeRegex().IsMatch(code))
            .WithMessage("Code must contain only lowercase letters, numbers, and underscores.");

        RuleFor(x => x.NameEn)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.NameAm)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DescriptionEn)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.DescriptionAm)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.IconKey)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.IconKey));
    }

    [GeneratedRegex("^[a-z0-9_]+$")]
    private static partial Regex CampaignTypeCodeRegex();
}
