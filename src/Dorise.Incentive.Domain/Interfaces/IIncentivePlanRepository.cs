using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for IncentivePlan aggregate.
/// "That's where I saw the leprechaun. He tells me to burn things." - And this repo tells me to fetch plans!
/// </summary>
public interface IIncentivePlanRepository : IAggregateRepository<IncentivePlan>
{
    /// <summary>
    /// Gets an incentive plan by its unique code.
    /// </summary>
    Task<IncentivePlan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plans by status.
    /// </summary>
    Task<IReadOnlyList<IncentivePlan>> GetByStatusAsync(
        PlanStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plans by type.
    /// </summary>
    Task<IReadOnlyList<IncentivePlan>> GetByTypeAsync(
        PlanType planType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active plans effective on a given date.
    /// </summary>
    Task<IReadOnlyList<IncentivePlan>> GetActivePlansAsync(
        DateTime effectiveDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a plan with its slabs.
    /// </summary>
    Task<IncentivePlan?> GetWithSlabsAsync(
        Guid planId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plans assigned to an employee.
    /// </summary>
    Task<IReadOnlyList<IncentivePlan>> GetByEmployeeAsync(
        Guid employeeId,
        DateTime? effectiveDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if plan code exists.
    /// </summary>
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plans with overlapping effective periods.
    /// </summary>
    Task<IReadOnlyList<IncentivePlan>> GetOverlappingPlansAsync(
        DateRange period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a slab by its ID.
    /// </summary>
    Task<Slab?> GetSlabByIdAsync(Guid slabId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets incentive plans with pagination and filtering.
    /// </summary>
    Task<(IReadOnlyList<IncentivePlan> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        PlanStatus? status = null,
        PlanType? planType = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
}
