using FluentValidation;

namespace Dorise.Incentive.Application.Calculations.Commands.RunCalculation;

/// <summary>
/// Validator for RunCalculationCommand.
/// </summary>
public class RunCalculationCommandValidator : AbstractValidator<RunCalculationCommand>
{
    public RunCalculationCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(x => x.IncentivePlanId)
            .NotEmpty().WithMessage("Incentive Plan ID is required.");

        RuleFor(x => x.PeriodStart)
            .NotEmpty().WithMessage("Period start date is required.")
            .LessThan(x => x.PeriodEnd).WithMessage("Period start must be before period end.");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty().WithMessage("Period end date is required.");

        RuleFor(x => x.ActualValue)
            .GreaterThanOrEqualTo(0).WithMessage("Actual value cannot be negative.");
    }
}
