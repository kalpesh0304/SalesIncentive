using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for PlanAssignment operations.
/// "Go banana!" - Fetching those assignments!
/// </summary>
public class PlanAssignmentRepository : IPlanAssignmentRepository
{
    private readonly IncentiveDbContext _context;

    public PlanAssignmentRepository(IncentiveDbContext context)
    {
        _context = context;
    }

    public async Task<PlanAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PlanAssignments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<PlanAssignment?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PlanAssignments
            .Include(a => a.Employee)
            .Include(a => a.IncentivePlan)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PlanAssignment>> GetByEmployeeAsync(
        Guid employeeId,
        bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PlanAssignments
            .Include(a => a.IncentivePlan)
            .Where(a => a.EmployeeId == employeeId);

        if (activeOnly)
        {
            query = query.Where(a => a.IsActive);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PlanAssignment>> GetByPlanAsync(
        Guid planId,
        bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PlanAssignments
            .Include(a => a.Employee)
            .Where(a => a.IncentivePlanId == planId);

        if (activeOnly)
        {
            query = query.Where(a => a.IsActive);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PlanAssignment>> GetByEmployeeAndPlanAsync(
        Guid employeeId,
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlanAssignments
            .Where(a => a.EmployeeId == employeeId && a.IncentivePlanId == planId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PlanAssignment>> GetFilteredAsync(
        Guid? employeeId = null,
        Guid? planId = null,
        bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PlanAssignments
            .Include(a => a.Employee)
            .Include(a => a.IncentivePlan)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (planId.HasValue)
        {
            query = query.Where(a => a.IncentivePlanId == planId.Value);
        }

        if (activeOnly)
        {
            query = query.Where(a => a.IsActive);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PlanAssignment> AddAsync(PlanAssignment assignment, CancellationToken cancellationToken = default)
    {
        await _context.PlanAssignments.AddAsync(assignment, cancellationToken);
        return assignment;
    }

    public void Update(PlanAssignment assignment)
    {
        _context.PlanAssignments.Update(assignment);
    }

    public void Remove(PlanAssignment assignment)
    {
        _context.PlanAssignments.Remove(assignment);
    }

    public async Task<bool> HasActiveAssignmentAsync(
        Guid employeeId,
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlanAssignments
            .AnyAsync(a => a.EmployeeId == employeeId &&
                         a.IncentivePlanId == planId &&
                         a.IsActive, cancellationToken);
    }
}
