using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class UpdateOrganizationProfileRequestValidator : AbstractValidator<UpdateOrganizationProfileRequest>
{
    public static readonly HashSet<string> AllowedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en-US",
        "am-ET"
    };

    public UpdateOrganizationProfileRequestValidator()
    {
        RuleFor(x => x.AgencyName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ContactEmail)
            .MaximumLength(256)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.PhysicalAddress)
            .MaximumLength(500);

        RuleFor(x => x.LogoUrl)
            .MaximumLength(2048)
            .Must(url => string.IsNullOrWhiteSpace(url) || BeAbsoluteUrl(url!))
            .WithMessage("LogoUrl must be a valid absolute URL.");

        RuleFor(x => x.LanguagePreference)
            .NotEmpty()
            .Must(lang => AllowedLanguages.Contains(lang.Trim()))
            .WithMessage("LanguagePreference must be en-US or am-ET.");
    }

    private static bool BeAbsoluteUrl(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
