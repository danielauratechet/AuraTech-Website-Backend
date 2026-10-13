using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class UpdateCampaignTypeRequestValidator : AbstractValidator<UpdateCampaignTypeRequest>
{
    public UpdateCampaignTypeRequestValidator()
    {
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
}
