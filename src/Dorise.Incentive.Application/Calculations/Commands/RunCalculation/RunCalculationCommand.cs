using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.Calculations.Commands.RunCalculation;

/// <summary>
/// Command to run an incentive calculation for an employee.
/// "I found a moon rock in my nose!" - And we found the right incentive!
/// </summary>
public record RunCalculationCommand : ICommand<Guid>
{
    public Guid EmployeeId { get; init; }
    public Guid IncentivePlanId { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal ActualValue { get; init; }
}
