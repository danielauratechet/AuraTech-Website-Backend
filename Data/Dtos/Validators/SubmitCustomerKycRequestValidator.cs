using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class SubmitCustomerKycRequestValidator : AbstractValidator<SubmitCustomerKycRequest>
{
    public SubmitCustomerKycRequestValidator()
    {
        RuleFor(x => x.TinCertificateUrl)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(BeAbsoluteUrl)
            .WithMessage("TinCertificateUrl must be a valid absolute URL.");

        RuleFor(x => x.GovernmentIdUrl)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(BeAbsoluteUrl)
            .WithMessage("GovernmentIdUrl must be a valid absolute URL.");
    }

    private static bool BeAbsoluteUrl(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
