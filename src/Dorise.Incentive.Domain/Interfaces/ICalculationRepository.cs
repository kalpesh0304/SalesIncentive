using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for Calculation aggregate.
/// "Then, the doctor told me that BOTH my eyes were lazy! And that's why it was
/// the best summer ever." - And this is the best repository ever!
/// </summary>
public interface ICalculationRepository : IAggregateRepository<Calculation>
{
    /// <summary>
    /// Gets calculations for an employee.
    /// </summary>
    Task<IReadOnlyList<Calculation>> GetByEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calculations for an employee in a specific period.
    /// </summary>
    Task<IReadOnlyList<Calculation>> GetByEmployeeAndPeriodAsync(
        Guid employeeId,
        DateRange period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calculations for an incentive plan.
    /// </summary>
    Task<IReadOnlyList<Calculation>> GetByPlanAsync(
        Guid planId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calculations by status.
    /// </summary>
    Task<IReadOnlyList<Calculation>> GetByStatusAsync(
        CalculationStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calculations pending approval for an approver.
    /// </summary>
    Task<IReadOnlyList<Calculation>> GetPendingApprovalForAsync(
        Guid approverId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a calculation with its approvals.
    /// </summary>
    Task<Calculation?> GetWithApprovalsAsync(
        Guid calculationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calculations for a period across all employees.
    /// </summary>
    Task<IReadOnlyList<Calculation>> GetForPeriodAsync(
        DateRange period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets latest calculation for an employee and plan.
    /// </summary>
    Task<Calculation?> GetLatestAsync(
        Guid employeeId,
        Guid planId,
        DateRange period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if calculation exists for employee, plan, and period.
    /// </summary>
    Task<bool> ExistsForPeriodAsync(
        Guid employeeId,
        Guid planId,
        DateRange period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sum of paid incentives for an employee in a period.
    /// </summary>
    Task<decimal> GetTotalPaidAsync(
        Guid employeeId,
        DateRange period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calculations for bulk payment processing.
    /// </summary>
    Task<IReadOnlyList<Calculation>> GetApprovedForPaymentAsync(
        DateRange period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a calculation with full details including approvals.
    /// </summary>
    Task<Calculation?> GetWithDetailsAsync(
        Guid calculationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated calculations with optional filtering.
    /// </summary>
    Task<(IReadOnlyList<Calculation> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? period = null,
        Guid? employeeId = null,
        Guid? planId = null,
        CalculationStatus? status = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calculations for an employee with optional date and status filtering.
    /// </summary>
    Task<IReadOnlyList<Calculation>> GetByEmployeeAsync(
        Guid employeeId,
        DateTime? periodStart,
        DateTime? periodEnd,
        CalculationStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calculations for a period with optional department/plan filtering.
    /// </summary>
    Task<IReadOnlyList<Calculation>> GetByPeriodAsync(
        DateTime periodStart,
        DateTime periodEnd,
        Guid? departmentId = null,
        Guid? planId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending approvals with pagination.
    /// </summary>
    Task<(IReadOnlyList<Calculation> Items, int TotalCount)> GetPendingApprovalsAsync(
        Guid? approverId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calculations for a period string (e.g., "2025-01").
    /// Converts period string to date range internally.
    /// </summary>
    Task<IReadOnlyList<Calculation>> GetByPeriodAsync(
        string period,
        CancellationToken cancellationToken = default);
}
