using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.Calculations.Commands.ApproveCalculation;

/// <summary>
/// Command to approve a calculation.
/// "Go banana!" - Approvals make everyone happy!
/// </summary>
public record ApproveCalculationCommand(Guid CalculationId, string? Comments) : ICommand<CalculationDto>;

/// <summary>
/// Command to reject a calculation.
/// </summary>
public record RejectCalculationCommand(Guid CalculationId, string Reason) : ICommand<CalculationDto>;
