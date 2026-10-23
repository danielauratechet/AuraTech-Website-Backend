using AuratechBackend.Data.Dtos.Requests;
using FluentValidation;

namespace AuratechBackend.Data.Dtos.Validators;

public sealed class SaveCampaignStep2RequestValidator : AbstractValidator<SaveCampaignStep2Request>
{
    public static readonly HashSet<string> AllowedLanguages = new(StringComparer.OrdinalIgnoreCase) { "Am", "En" };

    public static readonly HashSet<string> AllowedAgeRanges = new(StringComparer.Ordinal)
    {
        "18-24", "25-34", "35-44", "45-54", "55-64", "65+"
    };

    public SaveCampaignStep2RequestValidator()
    {
        RuleFor(x => x.TargetRegionIds)
            .NotEmpty()
            .WithMessage("At least one target region is required.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Target region ids must be unique.");

        RuleFor(x => x.ExcludeRegionIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Exclude region ids must be unique.");

        RuleFor(x => x)
            .Must(x => !x.TargetRegionIds.Intersect(x.ExcludeRegionIds).Any())
            .WithMessage("A region cannot be both targeted and excluded.")
            .WithName(nameof(SaveCampaignStep2Request.ExcludeRegionIds));

        RuleFor(x => x.Languages)
            .NotEmpty()
            .Must(langs => langs.All(l => AllowedLanguages.Contains(l.Trim())))
            .WithMessage("Languages must be Am and/or En.");

        RuleFor(x => x.AgeRanges)
            .NotEmpty()
            .Must(ages => ages.All(a => AllowedAgeRanges.Contains(a.Trim())))
            .WithMessage("One or more age ranges are invalid.");

        RuleFor(x => x.Gender)
            .IsInEnum();

        RuleFor(x => x.Interests)
            .NotEmpty()
            .Must(list => list.All(i => !string.IsNullOrWhiteSpace(i) && i.Trim().Length <= 100))
            .WithMessage("Each interest must be a non-empty string up to 100 characters.");
    }
}
