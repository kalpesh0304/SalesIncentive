using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Department aggregate.
/// "My cat's breath smells like cat food." - Departments smell like hierarchy!
/// </summary>
public class DepartmentRepository : AggregateRepositoryBase<Department>, IDepartmentRepository
{
    public DepartmentRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<Department?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(d => d.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> GetChildrenAsync(
        Guid parentId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(d => d.ParentId == parentId)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> GetHierarchyAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var result = new List<Department>();
        var current = await DbSet
            .Include(d => d.Parent)
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        while (current != null)
        {
            result.Add(current);
            if (current.ParentId == null) break;

            current = await DbSet
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(d => d.Id == current.ParentId, cancellationToken);
        }

        return result;
    }

    public async Task<IReadOnlyList<Department>> GetRootDepartmentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(d => d.ParentId == null && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Department?> GetWithEmployeesAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(d => d.Code == code, cancellationToken);
    }
}
