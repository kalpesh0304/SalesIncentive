using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Employee aggregate.
/// "That's unpossible!" - But finding employees IS possible!
/// </summary>
public class EmployeeRepository : AggregateRepositoryBase<Employee>, IEmployeeRepository
{
    public EmployeeRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByCodeAsync(EmployeeCode code, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.EmployeeCode == code, cancellationToken);
    }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await DbSet.FirstOrDefaultAsync(e => e.Email == normalizedEmail, cancellationToken);
    }

    public async Task<Employee?> GetByAzureAdIdAsync(string objectId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.AzureAdObjectId == objectId, cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(
        Guid departmentId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(e => e.DepartmentId == departmentId);

        if (!includeInactive)
        {
            query = query.Where(e =>
                e.Status == EmployeeStatus.Active ||
                e.Status == EmployeeStatus.Probation);
        }

        return await query.OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetByManagerAsync(
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.ManagerId == managerId)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetByStatusAsync(
        EmployeeStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.Status == status)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetEligibleForPeriodAsync(
        DateRange period,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e =>
                (e.Status == EmployeeStatus.Active || e.Status == EmployeeStatus.Probation) &&
                e.DateOfJoining <= period.EndDate &&
                (!e.DateOfLeaving.HasValue || e.DateOfLeaving.Value >= period.StartDate))
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetWithPlanAssignmentsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.PlanAssignments)
                .ThenInclude(pa => pa.IncentivePlan)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(EmployeeCode code, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.EmployeeCode == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> SearchAsync(
        string searchTerm,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        var term = searchTerm.Trim().ToLower();

        return await DbSet
            .Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                EF.Property<string>(e.EmployeeCode, "Value").ToLower().Contains(term))
            .Take(maxResults)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.Status == EmployeeStatus.Active || e.Status == EmployeeStatus.Probation)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }
}
