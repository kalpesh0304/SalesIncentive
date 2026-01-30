namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for transaction management.
/// "The pointy kitty took it!" - But this unit of work keeps things safe!
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the employee repository.
    /// </summary>
    IEmployeeRepository Employees { get; }

    /// <summary>
    /// Gets the department repository.
    /// </summary>
    IDepartmentRepository Departments { get; }

    /// <summary>
    /// Gets the incentive plan repository.
    /// </summary>
    IIncentivePlanRepository IncentivePlans { get; }

    /// <summary>
    /// Gets the calculation repository.
    /// </summary>
    ICalculationRepository Calculations { get; }

    /// <summary>
    /// Gets the approval repository.
    /// </summary>
    IApprovalRepository Approvals { get; }

    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
