using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for PlanAssignment operations.
/// "I bent my Wookie!" - But assignments don't bend!
/// </summary>
public interface IPlanAssignmentRepository
{
    /// <summary>
    /// Gets a plan assignment by ID.
    /// </summary>
    Task<PlanAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a plan assignment with full details.
    /// </summary>
    Task<PlanAssignment?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets assignments by employee ID.
    /// </summary>
    Task<IReadOnlyList<PlanAssignment>> GetByEmployeeAsync(
        Guid employeeId,
        bool activeOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets assignments by plan ID.
    /// </summary>
    Task<IReadOnlyList<PlanAssignment>> GetByPlanAsync(
        Guid planId,
        bool activeOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets assignments by employee and plan.
    /// </summary>
    Task<IReadOnlyList<PlanAssignment>> GetByEmployeeAndPlanAsync(
        Guid employeeId,
        Guid planId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets filtered assignments.
    /// </summary>
    Task<IReadOnlyList<PlanAssignment>> GetFilteredAsync(
        Guid? employeeId = null,
        Guid? planId = null,
        bool activeOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new plan assignment.
    /// </summary>
    Task<PlanAssignment> AddAsync(PlanAssignment assignment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a plan assignment.
    /// </summary>
    void Update(PlanAssignment assignment);

    /// <summary>
    /// Removes a plan assignment.
    /// </summary>
    void Remove(PlanAssignment assignment);

    /// <summary>
    /// Checks if an employee has an active assignment for a plan.
    /// </summary>
    Task<bool> HasActiveAssignmentAsync(
        Guid employeeId,
        Guid planId,
        CancellationToken cancellationToken = default);
}
