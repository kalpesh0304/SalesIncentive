using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for Employee aggregate.
/// "Eww, Daddy, this tastes like Gramma!" - But this repository tastes like quality code!
/// </summary>
public interface IEmployeeRepository : IAggregateRepository<Employee>
{
    /// <summary>
    /// Gets an employee by their unique code.
    /// </summary>
    Task<Employee?> GetByCodeAsync(EmployeeCode code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an employee by email address.
    /// </summary>
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an employee by Azure AD object ID.
    /// </summary>
    Task<Employee?> GetByAzureAdIdAsync(string objectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets employees by department.
    /// </summary>
    Task<IReadOnlyList<Employee>> GetByDepartmentAsync(
        Guid departmentId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets employees by their manager.
    /// </summary>
    Task<IReadOnlyList<Employee>> GetByManagerAsync(
        Guid managerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets employees by status.
    /// </summary>
    Task<IReadOnlyList<Employee>> GetByStatusAsync(
        EmployeeStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets employees eligible for incentive calculation in a period.
    /// </summary>
    Task<IReadOnlyList<Employee>> GetEligibleForPeriodAsync(
        DateRange period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets employee with their plan assignments.
    /// </summary>
    Task<Employee?> GetWithPlanAssignmentsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if employee code exists.
    /// </summary>
    Task<bool> CodeExistsAsync(EmployeeCode code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches employees by name or code.
    /// </summary>
    Task<IReadOnlyList<Employee>> SearchAsync(
        string searchTerm,
        int maxResults = 20,
        CancellationToken cancellationToken = default);
}
