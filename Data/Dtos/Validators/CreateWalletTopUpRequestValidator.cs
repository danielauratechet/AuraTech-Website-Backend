using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class CreateWalletTopUpRequestValidator : AbstractValidator<CreateWalletTopUpRequest>
{
    public CreateWalletTopUpRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true);

        RuleFor(x => x.CustomerName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerName));

        RuleFor(x => x.CustomerPhoneNumber)
            .NotEmpty()
            .Must(phone => EthiopianPhoneNumbers.TryNormalize(phone, out _))
            .WithMessage("CustomerPhoneNumber must be an Ethiopian mobile number.");

        RuleFor(x => x.CustomerEmail)
            .MaximumLength(256)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail));

        RuleFor(x => x.ReturnUrl)
            .MaximumLength(2048)
            .Must(BeAbsoluteHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.ReturnUrl))
            .WithMessage("ReturnUrl must be an absolute HTTP or HTTPS URL.");
    }

    private static bool BeAbsoluteHttpUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
