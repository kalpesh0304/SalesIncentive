using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.Calculations.Commands.VoidCalculation;

/// <summary>
/// Command to void a calculation.
/// "The red one tastes like burning!" - But voided calculations just disappear!
/// </summary>
public record VoidCalculationCommand(Guid CalculationId, string Reason) : ICommand;
