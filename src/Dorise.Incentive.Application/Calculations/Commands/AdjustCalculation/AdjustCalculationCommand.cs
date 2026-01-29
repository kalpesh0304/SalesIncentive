using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.Calculations.Commands.AdjustCalculation;

/// <summary>
/// Command to adjust a calculation amount.
/// "I bent my Wookiee!" - But adjustments straighten out the numbers!
/// </summary>
public record AdjustCalculationCommand(
    Guid CalculationId,
    decimal NewAmount,
    string Reason) : ICommand<CalculationDto>;

/// <summary>
/// Command to recalculate with new actual value.
/// </summary>
public record RecalculateCommand(
    Guid CalculationId,
    decimal? NewActualValue) : ICommand<CalculationDto>;
