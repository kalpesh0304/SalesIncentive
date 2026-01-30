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

    public async Task<Calculation?> GetWithDetailsAsync(
        Guid calculationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Employee)
            .Include(c => c.IncentivePlan)
                .ThenInclude(p => p!.Slabs)
            .Include(c => c.Approvals)
            .Include(c => c.AppliedSlab)
            .FirstOrDefaultAsync(c => c.Id == calculationId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Calculation> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? period = null,
        Guid? employeeId = null,
        Guid? planId = null,
        CalculationStatus? status = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(c => c.Employee)
            .Include(c => c.IncentivePlan)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(period))
        {
            // Parse period in format "yyyy-MM"
            if (DateTime.TryParse(period + "-01", out var periodDate))
            {
                var periodStart = new DateTime(periodDate.Year, periodDate.Month, 1);
                var periodEnd = periodStart.AddMonths(1).AddDays(-1);
                query = query.Where(c =>
                    c.CalculationPeriod.StartDate >= periodStart &&
                    c.CalculationPeriod.EndDate <= periodEnd);
            }
        }

        if (employeeId.HasValue)
        {
            query = query.Where(c => c.EmployeeId == employeeId.Value);
        }

        if (planId.HasValue)
        {
            query = query.Where(c => c.IncentivePlanId == planId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy?.ToLowerInvariant() switch
        {
            "employee" => sortDescending
                ? query.OrderByDescending(c => c.Employee!.FirstName).ThenByDescending(c => c.Employee!.LastName)
                : query.OrderBy(c => c.Employee!.FirstName).ThenBy(c => c.Employee!.LastName),
            "plan" => sortDescending
                ? query.OrderByDescending(c => c.IncentivePlan!.Name)
                : query.OrderBy(c => c.IncentivePlan!.Name),
            "amount" => sortDescending
                ? query.OrderByDescending(c => c.NetIncentive.Amount)
                : query.OrderBy(c => c.NetIncentive.Amount),
            "achievement" => sortDescending
                ? query.OrderByDescending(c => c.Achievement.Value)
                : query.OrderBy(c => c.Achievement.Value),
            "status" => sortDescending
                ? query.OrderByDescending(c => c.Status)
                : query.OrderBy(c => c.Status),
            _ => sortDescending
                ? query.OrderByDescending(c => c.CalculatedAt)
                : query.OrderBy(c => c.CalculatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Calculation>> GetByEmployeeAsync(
        Guid employeeId,
        DateTime? periodStart,
        DateTime? periodEnd,
        CalculationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(c => c.IncentivePlan)
            .Where(c => c.EmployeeId == employeeId);

        if (periodStart.HasValue)
        {
            query = query.Where(c => c.CalculationPeriod.StartDate >= periodStart.Value);
        }

        if (periodEnd.HasValue)
        {
            query = query.Where(c => c.CalculationPeriod.EndDate <= periodEnd.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        return await query
            .OrderByDescending(c => c.CalculatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Calculation>> GetByPeriodAsync(
        DateTime periodStart,
        DateTime periodEnd,
        Guid? departmentId = null,
        Guid? planId = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(c => c.Employee)
            .Include(c => c.IncentivePlan)
            .Where(c =>
                c.CalculationPeriod.StartDate >= periodStart &&
                c.CalculationPeriod.EndDate <= periodEnd);

        if (departmentId.HasValue)
        {
            query = query.Where(c => c.Employee!.DepartmentId == departmentId.Value);
        }

        if (planId.HasValue)
        {
            query = query.Where(c => c.IncentivePlanId == planId.Value);
        }

        return await query
            .OrderBy(c => c.Employee!.FirstName)
            .ThenBy(c => c.Employee!.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Calculation> Items, int TotalCount)> GetPendingApprovalsAsync(
        Guid? approverId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(c => c.Employee)
            .Include(c => c.IncentivePlan)
            .Include(c => c.Approvals)
            .Where(c => c.Status == CalculationStatus.PendingApproval);

        if (approverId.HasValue)
        {
            query = query.Where(c => c.Approvals.Any(a =>
                a.ApproverId == approverId.Value &&
                a.Status == ApprovalStatus.Pending));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CalculatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
