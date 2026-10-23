using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class SaveCampaignStep1RequestValidator : AbstractValidator<SaveCampaignStep1Request>
{
    public SaveCampaignStep1RequestValidator()
    {
        RuleFor(x => x.CampaignTypeId)
            .GreaterThan(0);
    }
}
