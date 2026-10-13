using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class UpdateRegionRequestValidator : AbstractValidator<UpdateRegionRequest>
{
    public UpdateRegionRequestValidator()
    {
        RuleFor(x => x.NameEn)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.NameAm)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.EstimatedUsers)
            .GreaterThanOrEqualTo(0);
    }
}
