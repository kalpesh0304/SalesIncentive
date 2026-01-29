using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Domain.Services;

/// <summary>
/// Domain service for validating incentive plan configurations.
/// "Even the Chinese don't WANT to see my butt!" - But everyone wants to see valid plans!
/// </summary>
public class PlanValidationService : IPlanValidationService
{
    public PlanValidationResult ValidatePlan(IncentivePlan plan)
    {
        var errors = new List<ValidationIssue>();
        var warnings = new List<ValidationIssue>();

        // Rule 1: Basic field validation
        ValidateBasicFields(plan, errors);

        // Rule 2: Effective period validation
        ValidateEffectivePeriod(plan, errors, warnings);

        // Rule 3: Target configuration validation
        ValidateTargetConfiguration(plan, errors, warnings);

        // Rule 4: Payout limits validation
        ValidatePayoutLimits(plan, errors);

        // Rule 5: Slab validation for slab-based plans
        if (plan.PlanType == PlanType.SlabBased)
        {
            var slabIssues = ValidateSlabs(plan);
            errors.AddRange(slabIssues.Where(i => IsCritical(i)));
            warnings.AddRange(slabIssues.Where(i => !IsCritical(i)));
        }

        // Rule 6: Approval settings validation
        ValidateApprovalSettings(plan, errors);

        if (errors.Count > 0)
        {
            return PlanValidationResult.Invalid(errors, warnings);
        }

        return PlanValidationResult.Valid(warnings);
    }

    public bool CanActivate(IncentivePlan plan)
    {
        var result = ValidatePlan(plan);
        return result.CanBeActivated;
    }

    public IReadOnlyList<ValidationIssue> ValidateSlabs(IncentivePlan plan)
    {
        var issues = new List<ValidationIssue>();

        if (plan.Slabs == null || plan.Slabs.Count == 0)
        {
            if (plan.PlanType == PlanType.SlabBased)
            {
                issues.Add(new ValidationIssue(
                    "SLAB_REQUIRED",
                    "Slab-based plans must have at least one slab configured",
                    "Slabs"));
            }
            return issues;
        }

        var orderedSlabs = plan.Slabs.OrderBy(s => s.Order).ToList();

        // Rule: Check for duplicate orders
        var duplicateOrders = orderedSlabs
            .GroupBy(s => s.Order)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateOrders.Count > 0)
        {
            issues.Add(new ValidationIssue(
                "DUPLICATE_SLAB_ORDER",
                $"Duplicate slab orders found: {string.Join(", ", duplicateOrders)}",
                "Slabs"));
        }

        // Rule: Check for overlapping ranges
        for (int i = 0; i < orderedSlabs.Count; i++)
        {
            for (int j = i + 1; j < orderedSlabs.Count; j++)
            {
                if (RangesOverlap(orderedSlabs[i], orderedSlabs[j]))
                {
                    issues.Add(new ValidationIssue(
                        "OVERLAPPING_SLAB_RANGES",
                        $"Slabs '{orderedSlabs[i].Name}' and '{orderedSlabs[j].Name}' have overlapping ranges",
                        "Slabs"));
                }
            }
        }

        // Rule: Check for gaps in ranges
        for (int i = 0; i < orderedSlabs.Count - 1; i++)
        {
            var current = orderedSlabs[i];
            var next = orderedSlabs[i + 1];

            if (Math.Abs(current.ToPercentage.Value - next.FromPercentage.Value) > 0.01m)
            {
                issues.Add(new ValidationIssue(
                    "GAP_IN_SLAB_RANGES",
                    $"Gap exists between slabs '{current.Name}' ({current.ToPercentage}%) and '{next.Name}' ({next.FromPercentage}%)",
                    "Slabs"));
            }
        }

        // Rule: First slab should start at or near 0 or minimum threshold
        if (orderedSlabs.Count > 0)
        {
            var firstSlab = orderedSlabs[0];
            if (firstSlab.FromPercentage.Value > plan.Target.MinimumThreshold + 1)
            {
                issues.Add(new ValidationIssue(
                    "FIRST_SLAB_START_HIGH",
                    $"First slab starts at {firstSlab.FromPercentage}% but minimum threshold is {plan.Target.MinimumThreshold}%",
                    "Slabs"));
            }
        }

        // Rule: Each slab should have valid payout
        foreach (var slab in orderedSlabs)
        {
            if (slab.PayoutPercentage.Value <= 0 && (slab.FixedAmount == null || !slab.FixedAmount.IsPositive()))
            {
                issues.Add(new ValidationIssue(
                    "SLAB_NO_PAYOUT",
                    $"Slab '{slab.Name}' has no payout configured (neither percentage nor fixed amount)",
                    $"Slabs[{slab.Order}]"));
            }
        }

        // Warning: Check if slabs cover up to 100% or beyond
        var lastSlab = orderedSlabs.LastOrDefault();
        if (lastSlab != null && lastSlab.ToPercentage.Value < 100)
        {
            issues.Add(new ValidationIssue(
                "SLABS_INCOMPLETE_COVERAGE",
                $"Slabs only cover up to {lastSlab.ToPercentage}%. Consider adding coverage for higher achievement.",
                "Slabs"));
        }

        return issues;
    }

    private void ValidateBasicFields(IncentivePlan plan, List<ValidationIssue> errors)
    {
        if (string.IsNullOrWhiteSpace(plan.Code))
        {
            errors.Add(new ValidationIssue("CODE_REQUIRED", "Plan code is required", "Code"));
        }

        if (string.IsNullOrWhiteSpace(plan.Name))
        {
            errors.Add(new ValidationIssue("NAME_REQUIRED", "Plan name is required", "Name"));
        }
    }

    private void ValidateEffectivePeriod(IncentivePlan plan, List<ValidationIssue> errors, List<ValidationIssue> warnings)
    {
        if (plan.EffectivePeriod.EndDate <= plan.EffectivePeriod.StartDate)
        {
            errors.Add(new ValidationIssue(
                "INVALID_PERIOD",
                "Effective end date must be after start date",
                "EffectivePeriod"));
        }

        if (plan.EffectivePeriod.EndDate <= DateTime.UtcNow.Date)
        {
            errors.Add(new ValidationIssue(
                "PERIOD_EXPIRED",
                "Plan effective period has already ended",
                "EffectivePeriod"));
        }

        // Warning if period is very short
        if (plan.EffectivePeriod.TotalDays < 30)
        {
            warnings.Add(new ValidationIssue(
                "SHORT_PERIOD",
                $"Plan effective period is only {plan.EffectivePeriod.TotalDays} days",
                "EffectivePeriod"));
        }
    }

    private void ValidateTargetConfiguration(IncentivePlan plan, List<ValidationIssue> errors, List<ValidationIssue> warnings)
    {
        if (plan.Target.TargetValue <= 0)
        {
            errors.Add(new ValidationIssue(
                "TARGET_REQUIRED",
                "Target value must be greater than zero",
                "Target"));
        }

        if (plan.Target.MinimumThreshold > plan.Target.TargetValue)
        {
            errors.Add(new ValidationIssue(
                "INVALID_THRESHOLD",
                "Minimum threshold cannot exceed target value",
                "MinimumThreshold"));
        }

        // Warning if threshold is very high
        var thresholdPercent = plan.Target.MinimumThreshold / plan.Target.TargetValue * 100;
        if (thresholdPercent > 80)
        {
            warnings.Add(new ValidationIssue(
                "HIGH_THRESHOLD",
                $"Minimum threshold is {thresholdPercent:F0}% of target. This may be difficult to achieve.",
                "MinimumThreshold"));
        }
    }

    private void ValidatePayoutLimits(IncentivePlan plan, List<ValidationIssue> errors)
    {
        if (plan.MaximumPayout != null && plan.MinimumPayout != null)
        {
            if (plan.MaximumPayout < plan.MinimumPayout)
            {
                errors.Add(new ValidationIssue(
                    "INVALID_PAYOUT_LIMITS",
                    "Maximum payout cannot be less than minimum payout",
                    "MaximumPayout"));
            }
        }
    }

    private void ValidateApprovalSettings(IncentivePlan plan, List<ValidationIssue> errors)
    {
        if (plan.RequiresApproval && plan.ApprovalLevels <= 0)
        {
            errors.Add(new ValidationIssue(
                "INVALID_APPROVAL_LEVELS",
                "Approval levels must be greater than zero when approval is required",
                "ApprovalLevels"));
        }

        if (plan.ApprovalLevels > 5)
        {
            errors.Add(new ValidationIssue(
                "TOO_MANY_APPROVALS",
                "Approval levels cannot exceed 5",
                "ApprovalLevels"));
        }
    }

    private bool RangesOverlap(Slab slab1, Slab slab2)
    {
        return slab1.FromPercentage.Value < slab2.ToPercentage.Value &&
               slab2.FromPercentage.Value < slab1.ToPercentage.Value;
    }

    private bool IsCritical(ValidationIssue issue)
    {
        // These issue codes are critical and prevent activation
        var criticalCodes = new[]
        {
            "SLAB_REQUIRED",
            "DUPLICATE_SLAB_ORDER",
            "OVERLAPPING_SLAB_RANGES",
            "CODE_REQUIRED",
            "NAME_REQUIRED",
            "INVALID_PERIOD",
            "PERIOD_EXPIRED",
            "TARGET_REQUIRED",
            "INVALID_THRESHOLD",
            "INVALID_PAYOUT_LIMITS",
            "INVALID_APPROVAL_LEVELS",
            "TOO_MANY_APPROVALS",
            "SLAB_NO_PAYOUT"
        };

        return criticalCodes.Contains(issue.Code);
    }
}
