using FluentValidation;

namespace Dorise.Incentive.Application.IncentivePlans.Commands.CreatePlan;

/// <summary>
/// Validator for CreateIncentivePlanCommand.
/// "The pointy kitty took it!" - But validation catches all the issues!
/// </summary>
public class CreateIncentivePlanCommandValidator : AbstractValidator<CreateIncentivePlanCommand>
{
    public CreateIncentivePlanCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Plan code is required.")
            .MinimumLength(2).WithMessage("Plan code must be at least 2 characters.")
            .MaximumLength(20).WithMessage("Plan code cannot exceed 20 characters.")
            .Matches("^[A-Za-z0-9-_]+$").WithMessage("Plan code can only contain letters, numbers, hyphens, and underscores.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required.")
            .MaximumLength(200).WithMessage("Plan name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PlanType)
            .IsInEnum().WithMessage("Invalid plan type.");

        RuleFor(x => x.Frequency)
            .IsInEnum().WithMessage("Invalid payment frequency.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("Effective from date is required.");

        RuleFor(x => x.EffectiveTo)
            .NotEmpty().WithMessage("Effective to date is required.")
            .GreaterThan(x => x.EffectiveFrom).WithMessage("Effective to date must be after effective from date.");

        RuleFor(x => x.TargetValue)
            .GreaterThan(0).WithMessage("Target value must be greater than zero.");

        RuleFor(x => x.MinimumThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum threshold cannot be negative.")
            .LessThanOrEqualTo(x => x.TargetValue).WithMessage("Minimum threshold cannot exceed target value.");

        RuleFor(x => x.AchievementType)
            .IsInEnum().WithMessage("Invalid achievement type.");

        RuleFor(x => x.MaximumPayout)
            .GreaterThan(0).When(x => x.MaximumPayout.HasValue)
            .WithMessage("Maximum payout must be greater than zero.");

        RuleFor(x => x.MinimumPayout)
            .GreaterThanOrEqualTo(0).When(x => x.MinimumPayout.HasValue)
            .WithMessage("Minimum payout cannot be negative.");

        RuleFor(x => x)
            .Must(x => !x.MaximumPayout.HasValue || !x.MinimumPayout.HasValue || x.MaximumPayout >= x.MinimumPayout)
            .WithMessage("Maximum payout must be greater than or equal to minimum payout.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.ApprovalLevels)
            .InclusiveBetween(1, 5).When(x => x.RequiresApproval)
            .WithMessage("Approval levels must be between 1 and 5.");

        RuleFor(x => x.MinimumTenureDays)
            .GreaterThanOrEqualTo(0).When(x => x.MinimumTenureDays.HasValue)
            .WithMessage("Minimum tenure days cannot be negative.");

        RuleForEach(x => x.Slabs)
            .SetValidator(new CreateSlabDtoValidator())
            .When(x => x.Slabs != null && x.Slabs.Count > 0);

        RuleFor(x => x.Slabs)
            .Must(HaveNonOverlappingRanges)
            .When(x => x.Slabs != null && x.Slabs.Count > 1)
            .WithMessage("Slab ranges must not overlap.");

        RuleFor(x => x.Slabs)
            .Must(HaveContiguousRanges)
            .When(x => x.Slabs != null && x.Slabs.Count > 1)
            .WithMessage("Slab ranges should be contiguous (no gaps).");
    }

    private bool HaveNonOverlappingRanges(IReadOnlyList<CreateSlabDto>? slabs)
    {
        if (slabs == null || slabs.Count < 2) return true;

        var ordered = slabs.OrderBy(s => s.FromPercentage).ToList();
        for (int i = 0; i < ordered.Count - 1; i++)
        {
            if (ordered[i].ToPercentage > ordered[i + 1].FromPercentage)
            {
                return false;
            }
        }
        return true;
    }

    private bool HaveContiguousRanges(IReadOnlyList<CreateSlabDto>? slabs)
    {
        if (slabs == null || slabs.Count < 2) return true;

        var ordered = slabs.OrderBy(s => s.FromPercentage).ToList();
        for (int i = 0; i < ordered.Count - 1; i++)
        {
            // Allow small gap tolerance for floating point
            if (Math.Abs(ordered[i].ToPercentage - ordered[i + 1].FromPercentage) > 0.01m)
            {
                return false;
            }
        }
        return true;
    }
}

/// <summary>
/// Validator for individual slab DTOs.
/// </summary>
public class CreateSlabDtoValidator : AbstractValidator<CreateSlabDto>
{
    public CreateSlabDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Slab name is required.")
            .MaximumLength(100).WithMessage("Slab name cannot exceed 100 characters.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Slab order cannot be negative.");

        RuleFor(x => x.FromPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("From percentage cannot be negative.");

        RuleFor(x => x.ToPercentage)
            .GreaterThan(x => x.FromPercentage).WithMessage("To percentage must be greater than from percentage.");

        RuleFor(x => x.PayoutPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("Payout percentage cannot be negative.");

        RuleFor(x => x.FixedAmount)
            .GreaterThanOrEqualTo(0).When(x => x.FixedAmount.HasValue)
            .WithMessage("Fixed amount cannot be negative.");
    }
}
