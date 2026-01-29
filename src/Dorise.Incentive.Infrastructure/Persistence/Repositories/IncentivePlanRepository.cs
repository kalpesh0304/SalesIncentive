using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for IncentivePlan aggregate.
/// "I like men now!" - I like plans now! And their slabs!
/// </summary>
public class IncentivePlanRepository : AggregateRepositoryBase<IncentivePlan>, IIncentivePlanRepository
{
    public IncentivePlanRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<IncentivePlan?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<IncentivePlan>> GetByStatusAsync(
        PlanStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.Status == status)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IncentivePlan>> GetByTypeAsync(
        PlanType planType,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.PlanType == planType)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IncentivePlan>> GetActivePlansAsync(
        DateTime effectiveDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p =>
                p.Status == PlanStatus.Active &&
                p.EffectivePeriod.StartDate <= effectiveDate &&
                (p.EffectivePeriod.EndDate == null || p.EffectivePeriod.EndDate >= effectiveDate))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IncentivePlan?> GetWithSlabsAsync(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Slabs.OrderBy(s => s.MinThreshold))
            .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
    }

    public async Task<IReadOnlyList<IncentivePlan>> GetByEmployeeAsync(
        Guid employeeId,
        DateTime? effectiveDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(p => p.Assignments)
            .Where(p => p.Assignments.Any(a => a.EmployeeId == employeeId));

        if (effectiveDate.HasValue)
        {
            query = query.Where(p =>
                p.Status == PlanStatus.Active &&
                p.EffectivePeriod.StartDate <= effectiveDate.Value &&
                (p.EffectivePeriod.EndDate == null || p.EffectivePeriod.EndDate >= effectiveDate.Value));
        }

        return await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<IncentivePlan>> GetOverlappingPlansAsync(
        DateRange period,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p =>
                p.EffectivePeriod.StartDate <= period.EndDate &&
                (p.EffectivePeriod.EndDate == null || p.EffectivePeriod.EndDate >= period.StartDate))
            .OrderBy(p => p.EffectivePeriod.StartDate)
            .ToListAsync(cancellationToken);
    }
}
