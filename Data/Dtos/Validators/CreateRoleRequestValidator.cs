using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
        RuleFor(x => x.PermissionIds).NotNull();
    }
}
