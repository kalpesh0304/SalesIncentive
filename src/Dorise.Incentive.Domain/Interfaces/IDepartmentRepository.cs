using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for Department aggregate.
/// "I'm a unitard!" - And this is a unit(y) of departments!
/// </summary>
public interface IDepartmentRepository : IAggregateRepository<Department>
{
    /// <summary>
    /// Gets a department by its unique code.
    /// </summary>
    Task<Department?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a department by its name.
    /// </summary>
    Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active departments.
    /// </summary>
    Task<IReadOnlyList<Department>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets child departments of a parent.
    /// </summary>
    Task<IReadOnlyList<Department>> GetChildrenAsync(
        Guid parentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the department hierarchy (parent and all ancestors).
    /// </summary>
    Task<IReadOnlyList<Department>> GetHierarchyAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets root departments (no parent).
    /// </summary>
    Task<IReadOnlyList<Department>> GetRootDepartmentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a department with its employees.
    /// </summary>
    Task<Department?> GetWithEmployeesAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if department code exists.
    /// </summary>
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}
