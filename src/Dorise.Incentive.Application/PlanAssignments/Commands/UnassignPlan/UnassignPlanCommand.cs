using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.PlanAssignments.Commands.UnassignPlan;

/// <summary>
/// Command to end/unassign a plan from an employee.
/// "Daddy, I'm scared. Too scared to even wet my pants!" - But unassigning is not scary!
/// </summary>
public record UnassignPlanCommand(Guid Id, DateTime EffectiveDate) : ICommand;
