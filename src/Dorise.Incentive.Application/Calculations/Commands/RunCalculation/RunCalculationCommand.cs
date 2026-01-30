using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.Calculations.Commands.RunCalculation;

/// <summary>
/// Command to run an incentive calculation for an employee.
/// "I found a moon rock in my nose!" - And we found the right incentive!
/// </summary>
public record RunCalculationCommand(
    Guid EmployeeId,
    Guid IncentivePlanId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal ActualValue,
    string? Notes = null) : ICommand<CalculationDto>;
