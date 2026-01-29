using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.PlanAssignments.DTOs;

namespace Dorise.Incentive.Application.PlanAssignments.Commands.AssignPlan;

/// <summary>
/// Command to assign an incentive plan to an employee.
/// "The leprechaun tells me to burn things!" - But we just assign plans, not burn them!
/// </summary>
public record AssignPlanToEmployeeCommand(
    Guid EmployeeId,
    Guid IncentivePlanId,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo = null,
    decimal? CustomTarget = null,
    string? CustomTargetUnit = null,
    string? Notes = null) : ICommand<PlanAssignmentDto>;

/// <summary>
/// Command to bulk assign a plan to multiple employees.
/// </summary>
public record BulkAssignPlanCommand(
    Guid IncentivePlanId,
    IReadOnlyList<Guid> EmployeeIds,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo = null) : ICommand<BulkAssignmentResultDto>;

/// <summary>
/// Command to update an existing plan assignment.
/// </summary>
public record UpdatePlanAssignmentCommand(
    Guid Id,
    DateTime? EffectiveTo = null,
    decimal? CustomTarget = null,
    string? CustomTargetUnit = null,
    string? Notes = null) : ICommand<PlanAssignmentDto>;
