using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Calculation aggregate.
/// "Look, Daddy! I made a calculation!" - And this repo stores them!
/// </summary>
public class CalculationRepository : AggregateRepositoryBase<Calculation>, ICalculationRepository
{
    public CalculationRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Calculation>> GetByEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.IncentivePlan)
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.CalculatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Calculation>> GetByEmployeeAndPeriodAsync(
        Guid employeeId,
        DateRange period,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.IncentivePlan)
            .Where(c =>
                c.EmployeeId == employeeId &&
                c.CalculationPeriod.StartDate >= period.StartDate &&
                c.CalculationPeriod.EndDate <= period.EndDate)
            .OrderByDescending(c => c.CalculatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Calculation>> GetByPlanAsync(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Employee)
            .Where(c => c.IncentivePlanId == planId)
            .OrderByDescending(c => c.CalculatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Calculation>> GetByStatusAsync(
        CalculationStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Employee)
            .Include(c => c.IncentivePlan)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CalculatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Calculation>> GetPendingApprovalForAsync(
        Guid approverId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Employee)
            .Include(c => c.IncentivePlan)
            .Include(c => c.Approvals)
            .Where(c =>
                c.Status == CalculationStatus.PendingApproval &&
                c.Approvals.Any(a =>
                    a.ApproverId == approverId &&
                    a.Status == ApprovalStatus.Pending))
            .OrderByDescending(c => c.CalculatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Calculation?> GetWithApprovalsAsync(
        Guid calculationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Employee)
            .Include(c => c.IncentivePlan)
            .Include(c => c.Approvals)
                .ThenInclude(a => a.Approver)
            .FirstOrDefaultAsync(c => c.Id == calculationId, cancellationToken);
    }

    public async Task<IReadOnlyList<Calculation>> GetForPeriodAsync(
        DateRange period,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Employee)
            .Include(c => c.IncentivePlan)
            .Where(c =>
                c.CalculationPeriod.StartDate >= period.StartDate &&
                c.CalculationPeriod.EndDate <= period.EndDate)
            .OrderBy(c => c.Employee!.FirstName)
            .ThenBy(c => c.Employee!.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Calculation?> GetLatestAsync(
        Guid employeeId,
        Guid planId,
        DateRange period,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c =>
                c.EmployeeId == employeeId &&
                c.IncentivePlanId == planId &&
                c.CalculationPeriod.StartDate == period.StartDate &&
                c.CalculationPeriod.EndDate == period.EndDate)
            .OrderByDescending(c => c.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsForPeriodAsync(
        Guid employeeId,
        Guid planId,
        DateRange period,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(c =>
            c.EmployeeId == employeeId &&
            c.IncentivePlanId == planId &&
            c.CalculationPeriod.StartDate == period.StartDate &&
            c.CalculationPeriod.EndDate == period.EndDate,
            cancellationToken);
    }

    public async Task<decimal> GetTotalPaidAsync(
        Guid employeeId,
        DateRange period,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c =>
                c.EmployeeId == employeeId &&
                c.Status == CalculationStatus.Paid &&
                c.CalculationPeriod.StartDate >= period.StartDate &&
                c.CalculationPeriod.EndDate <= period.EndDate)
            .SumAsync(c => c.NetIncentive.Amount, cancellationToken);
    }

    public async Task<IReadOnlyList<Calculation>> GetApprovedForPaymentAsync(
        DateRange period,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Employee)
            .Include(c => c.IncentivePlan)
            .Where(c =>
                c.Status == CalculationStatus.Approved &&
                c.CalculationPeriod.StartDate >= period.StartDate &&
                c.CalculationPeriod.EndDate <= period.EndDate)
            .OrderBy(c => c.Employee!.FirstName)
            .ThenBy(c => c.Employee!.LastName)
            .ToListAsync(cancellationToken);
    }
}
