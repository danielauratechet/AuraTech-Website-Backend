using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class UpdateOrganizationUserRequestValidator : AbstractValidator<UpdateOrganizationUserRequest>
{
    public UpdateOrganizationUserRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
