using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.DTOs;

namespace Dorise.Incentive.Application.IncentivePlans.Commands.ActivatePlan;

/// <summary>
/// Command to activate an incentive plan.
/// "Go banana!" - Plan activation makes things happen!
/// </summary>
public record ActivateIncentivePlanCommand(Guid Id) : ICommand<IncentivePlanDto>;

/// <summary>
/// Command to suspend an active plan.
/// </summary>
public record SuspendIncentivePlanCommand(Guid Id, string Reason) : ICommand<IncentivePlanDto>;

/// <summary>
/// Command to cancel a plan.
/// </summary>
public record CancelIncentivePlanCommand(Guid Id, string Reason) : ICommand<IncentivePlanDto>;

/// <summary>
/// Command to clone an existing plan.
/// </summary>
public record CloneIncentivePlanCommand(Guid SourceId, string NewCode, string NewName) : ICommand<IncentivePlanDto>;
