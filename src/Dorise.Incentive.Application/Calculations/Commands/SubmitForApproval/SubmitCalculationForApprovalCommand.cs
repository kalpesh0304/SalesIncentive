using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.Calculations.Commands.SubmitForApproval;

/// <summary>
/// Command to submit a calculation for approval.
/// "Even Zaius couldn't get me out of this jam!" - Submit those calculations!
/// </summary>
public record SubmitCalculationForApprovalCommand(Guid CalculationId) : ICommand;
