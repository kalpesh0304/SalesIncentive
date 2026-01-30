using Dorise.Incentive.Application.PlanAssignments.DTOs;
using MediatR;

namespace Dorise.Incentive.Application.PlanAssignments.Queries;

/// <summary>
/// Query to get plan assignments with filtering.
/// </summary>
public record GetPlanAssignmentsQuery(
    Guid? EmployeeId,
    Guid? PlanId,
    bool ActiveOnly) : IRequest<IReadOnlyList<PlanAssignmentDto>>;

/// <summary>
/// Query to get a specific assignment by ID.
/// </summary>
public record GetPlanAssignmentByIdQuery(Guid Id) : IRequest<PlanAssignmentDto?>;

/// <summary>
/// Query to get all assignments for an employee.
/// </summary>
public record GetEmployeeAssignmentsQuery(
    Guid EmployeeId,
    bool ActiveOnly) : IRequest<IReadOnlyList<PlanAssignmentDto>>;

/// <summary>
/// Query to check plan eligibility for an employee.
/// </summary>
public record CheckPlanEligibilityQuery(
    Guid EmployeeId,
    Guid PlanId,
    DateTime AsOfDate) : IRequest<EligibilityCheckResultDto>;
