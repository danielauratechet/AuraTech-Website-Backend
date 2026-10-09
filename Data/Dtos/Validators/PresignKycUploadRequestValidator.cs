using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class PresignKycUploadRequestValidator : AbstractValidator<PresignKycUploadRequest>
{
    private static readonly string[] AllowedKinds = ["tin_certificate", "government_id"];

    public PresignKycUploadRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Kind)
            .NotEmpty()
            .Must(kind => AllowedKinds.Contains(kind, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Kind must be 'tin_certificate' or 'government_id'.");

        RuleFor(x => x.ContentType)
            .MaximumLength(128);
    }
}
