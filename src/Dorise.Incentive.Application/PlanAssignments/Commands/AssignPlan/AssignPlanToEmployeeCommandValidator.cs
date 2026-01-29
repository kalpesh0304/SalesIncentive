using FluentValidation;

namespace Dorise.Incentive.Application.PlanAssignments.Commands.AssignPlan;

/// <summary>
/// Validator for AssignPlanToEmployeeCommand.
/// </summary>
public class AssignPlanToEmployeeCommandValidator : AbstractValidator<AssignPlanToEmployeeCommand>
{
    public AssignPlanToEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(x => x.IncentivePlanId)
            .NotEmpty().WithMessage("Incentive Plan ID is required.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("Effective from date is required.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("Effective to date must be after effective from date.");

        RuleFor(x => x.CustomTarget)
            .GreaterThan(0)
            .When(x => x.CustomTarget.HasValue)
            .WithMessage("Custom target must be greater than zero.");

        RuleFor(x => x.CustomTargetUnit)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.CustomTargetUnit))
            .WithMessage("Custom target unit cannot exceed 50 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters.");
    }
}
